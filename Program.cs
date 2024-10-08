using Alarm_v2;
using NAudio.Wave;
using System.CommandLine;
using static Alarm_v2.AppConfig;

Environment.CurrentDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory;
var root = new RootCommand();

// ROOT
var run = new Command("run", "运行闹钟");
var rc = new Command("rc", "运行远程控制程序");
var ls = new Command("show", "查看...");
root.AddCommand(run);
root.AddCommand(rc);
root.AddCommand(ls);

// RUN
var playlist = new Argument<FileInfo>("playlist", "播放列表文件");
var device = new Argument<Guid>("device", () => Guid.Empty, "输出设备");
run.AddArgument(playlist);
run.AddArgument(device);

var vol = new Option<int>("--volume", () => -1, "设定音量（并取消静音）");
vol.AddAlias("-sv");
run.AddOption(vol);

var pb = new Option<FileInfo>("--pb", "防重复功能");
pb.AddAlias("-pb");
run.AddOption(pb);
var pl = new Option<bool>("--pl", "防近似功能");
pl.AddAlias("-pl");
run.AddOption(pl);

var pfx = new Option<string[]>("--http-pfx", "HTTP控制URL前缀");
pfx.AddAlias("-pfx");
run.AddOption(pfx);

var log = new Option<FileInfo>("--log", () => new FileInfo("history.log"), "历史记录位置");
log.AddAlias("-log");
run.AddOption(log);

run.SetHandler((
    FileInfo lf,
    Guid dev,
    int v,
    FileInfo? pbf,
    bool ple,
    string[] pfxs,
    FileInfo? logf) =>
{
    // 构建 I/O 组件
    IO io = new()
    {
        HistoryPath = logf?.FullName ?? ""
    };

    // 读列表
    string[] list = M3u8.ReadList(lf.FullName);

    // 初始化播放器
    DirectSoundOut outDevice = new(dev);
    Player player = new(outDevice);

    // 列出设备
    VolumeHelper.ListDevice(io.WriteText);
    io.WriteText("Current     : " + dev.ToString());

    // 构建 PL 组件与输出通道
    PL? pl = ple ? new PL() : null;
    pl?.Add(list);
    Func<string> plnx = pl is null ?
        () => list[Random.Shared.Next(list.Length)] :
        pl.GetItem;

    // 构建 PB 组件与输出通道
    PB? pb = pbf is null ? null : new PB(pbf.FullName, plnx);
    Func<string> pbnx = pb is null ?
        plnx :
        pb.GetItem;

    // 构建主控制器
    Controller ctrl = new(player, pbnx);

    // 将 PB 组件连接到控制器
    if (pb is not null)
    {
        ctrl.OnSec += pb.IncAsync;
        ctrl.BeforeNext += (_) => pb.Save();
        OnExit += (_) => pb.Save();
    }

    // 历史记录输出
    ctrl.OnNext += (s) => io.WriteHistory(Path.GetFileNameWithoutExtension(s));
    // 退出流程
    ctrl.OnStop += () => Exit(0);

    // 构建网络控制器
    HttpController httpController = new(ctrl, io);
    foreach (var pfx in pfxs)
    {
        httpController.listener.Prefixes.Add(pfx);
    }

    // 音量调节
    OnExit += (_) => VolumeHelper.Shared.Foreach(VolumeHelper.Shared.Restore);
    if (v >= 0)
    {
        VolumeHelper.Shared.Foreach(VolumeHelper.Shared.SaveAndSet(v / 100.0f, false),
            dev == Guid.Empty ? "" : dev.ToString());
        io.WriteText("Volume      : " + v);
    }
    io.WriteText("");

    // 打印 PL & PB 信息
    if (pl is not null)
    {
        io.WriteText($"ADCC        : {pl.CalcuateADCC():F2}");
    }
    if (pbf is not null)
    {
        io.WriteText($"PB Path     : {pbf.FullName}");
    }
    io.WriteText("");

    // 启动
    ctrl.Start();
    if (httpController.listener.Prefixes.Count > 0)
    {
        httpController.Start();
        foreach(var pfxstr in httpController.listener.Prefixes)
        {
            io.WriteText($"HTTP RC URL : {pfxstr}");
        }
        io.WriteText("");
    }
    ConsoleHandler(ctrl.Pass, ctrl.Stop);
}, playlist, device, vol, pb, pl, pfx, log);

// RC
var uri = new Argument<string>("uri", "服务端URI");
rc.AddArgument(uri);

rc.SetHandler(uristr =>
{
    RC rcu = new();
    rcu.StartGet(uristr);
    ConsoleHandler(() => rcu.Pass(uristr), async () =>
    {
        var resp = await rcu.Stop(uristr);
        Exit(0);
    });
}, uri);

// SHOW
var ld = new Command("devices", "查看音频设备");
ld.SetHandler(() =>
{
    VolumeHelper.ListDevice(Console.WriteLine);
});
ls.AddCommand(ld);

var lpb = new Command("pb", "查看 PB 数据");
var lpb_pbf = new Argument<FileInfo>("pb", "PB 数据文件 [json]");
var name_width = new Option<int>("--name-width", () => Console.WindowWidth / 2, "名称部分长度");
name_width.AddAlias("-w");
lpb.AddArgument(lpb_pbf);
lpb.AddOption(name_width);
lpb.SetHandler((FileInfo pbf1, int nw) =>
{
    PB pb1 = new(pbf1.FullName, () => "");
    const int lnT = 4;
    const int lnP = 8;
    Console.WriteLine($"{"文件名".PadRight(nw-3)} 计时 通过概率");
    Console.WriteLine($"{"".PadLeft(nw, '-')} ---- --------");
    foreach(var (fn, t) in pb1.GetData())
    {
        Console.Write(fn);
        Console.CursorLeft = nw;
        Console.WriteLine($" {t,lnT} {PB.P(t),lnP:P2}".PadRight(Console.WindowWidth - nw));
    }
}, lpb_pbf, name_width);
ls.AddCommand(lpb);

var lpl = new Command("pl", "查看 PL 分析信息");
lpl.AddArgument(playlist);
lpl.AddOption(name_width);

lpl.SetHandler((FileInfo playlist2, int nw) =>
{
    string[] li2 = M3u8.ReadList(playlist2.FullName);
    PL pl = new();
    pl.Add(li2);
    double adcc = pl.CalcuateADCC();
    Console.WriteLine();
    Console.WriteLine($"ADCC = {adcc:F2}");
    Console.WriteLine();
    Console.WriteLine($"{"Path".PadRight(nw)} Count   DCC");
    Console.WriteLine($"{"".PadLeft(nw, '-')} ----- -----");
    pl.tree.PrintTree("", (tree) =>
    {
        (int l, int t) = Console.GetCursorPosition();
        Console.CursorLeft = nw;
        int dcc = tree.DirectChildrenCount();
        IO.CWrite($" {(dcc == tree.Count ? null : tree.Count),5} {(dcc == 0 ? null: dcc),5}", l, t);
    });
}, playlist, name_width);
ls.AddCommand(lpl);

root.Invoke(args);

void ConsoleHandler(Action onPass, Action onStop)
{

    Console.CancelKeyPress += (s, e) =>
    {
        e.Cancel = true;
        onStop();
    };

    while (true)
    {
        var k = Console.ReadKey(true);
        if (k.Modifiers.HasFlag(ConsoleModifiers.Control) && k.Key == ConsoleKey.N)
        {
            onPass();
        }
    }
}