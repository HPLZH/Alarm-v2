using Alarm_v2;
using NAudio.Wave;
using System.CommandLine;
using static Alarm_v2.AppConfig;

Environment.CurrentDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory;
var root = new RootCommand();
Config? sharedConfig = null;

// ROOT
var run = new Command("run", "运行闹钟");
var start = new Command("start", "通过配置文件运行闹钟");
var mkconf = new Command("make-config", "根据命令行参数生成配置文件");
var rc = new Command("rc", "运行远程控制程序");
var ls = new Command("show", "查看...");
root.AddCommand(run);
root.AddCommand(start);
root.AddCommand(mkconf);
root.AddCommand(rc);
root.AddCommand(ls);

// MAKE CONFIG
mkconf.AddAlias("mkconf");

// START
var conf = new Argument<FileInfo>("config-file", "配置文件");
start.AddArgument(conf);
start.SetHandler((FileInfo fc) =>
{
    var config = Config.Deserialize(fc) ?? throw new NullReferenceException("配置文件无效");
    sharedConfig = config;
    MainHandler(
        config.PlayList(),
        config.device,
        config.volume,
        config.PBFile(),
        config.pl,
        config.pfx,
        config.LogFile());
}, conf);

// RUN
var playlist = new Argument<FileInfo>("playlist", "播放列表文件");
var playlist_s = new Argument<string>("playlist", "播放列表文件");
var device = new Argument<Guid>("device", () => Guid.Empty, "输出设备");
run.AddArgument(playlist);
run.AddArgument(device);
mkconf.AddArgument(playlist_s);
mkconf.AddArgument(device);

var vol = new Option<int>("--volume", () => -1, "设定音量（并取消静音）");
vol.AddAlias("-sv");
run.AddOption(vol);
mkconf.AddOption(vol);

var pb = new Option<FileInfo>("--pb", "防重复功能");
var pb_s = new Option<string>("--pb", "防重复功能");
pb.AddAlias("-pb");
pb_s.AddAlias("-pb");
run.AddOption(pb);
mkconf.AddOption(pb_s);

var pl = new Option<bool>("--pl", "防近似功能");
pl.AddAlias("-pl");
run.AddOption(pl);
mkconf.AddOption(pl);

var pfx = new Option<string[]>("--http-pfx", "HTTP控制URL前缀");
pfx.AddAlias("-pfx");
run.AddOption(pfx);
mkconf.AddOption(pfx);

var log = new Option<FileInfo>("--log", () => new FileInfo("history.log"), "历史记录位置");
var log_s = new Option<string>("--log", () => "history.log", "历史记录位置");
log.AddAlias("-log");
log_s.AddAlias("-log");
run.AddOption(log);
mkconf.AddOption(log_s);

run.SetHandler(MainHandler, playlist, device, vol, pb, pl, pfx, log);
mkconf.SetHandler((
    string lf,
    Guid dev,
    int v,
    string? pbf,
    bool ple,
    string[] pfxs,
    string? logf) =>
{
    Console.WriteLine(new Config()
    {
        playlist = lf,
        device = dev,
        volume = v,
        pb = pbf,
        pl = ple,
        pfx = pfxs,
        log = logf
    }.Serialize());
}, playlist_s, device, vol, pb_s, pl, pfx, log_s);

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
    Console.WriteLine($"{"文件名".PadRight(nw - 3)} 计时 通过概率");
    Console.WriteLine($"{"".PadLeft(nw, '-')} ---- --------");
    foreach (var (fn, t) in pb1.GetData())
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
        IO.CWrite($" {(dcc == tree.Count ? null : tree.Count),5} {(dcc == 0 ? null : dcc),5}", l, t);
    });
}, playlist, name_width);
ls.AddCommand(lpl);

var lic = new Command("license", "查看许可证");
var dep = new Command("deps", "查看依赖项信息");
lic.SetHandler(License.Show);
dep.SetHandler(License.ShowDeps);
ls.AddCommand(lic);
ls.AddCommand(dep);


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

void MainHandler(
    FileInfo lf,
    Guid dev,
    int v,
    FileInfo? pbf,
    bool ple,
    string[] pfxs,
    FileInfo? logf)
{
    // 构建 I/O 组件
    IO io = new()
    {
        HistoryPath = logf?.FullName ?? ""
    };

    // 读列表
    string[] list = M3u8.ReadList(lf.FullName);

    // 初始化播放器
    bool devex = false;
    foreach(var d in DirectSoundOut.Devices)
    {
        if(d.Guid == dev)
        {
            devex = true;
            break;
        }
    }
    if (!devex)
    {
        dev = Guid.Empty;
    }
    DirectSoundOut outDevice = new(dev);
    Player player = new(outDevice, sharedConfig?.opts?.memstream ?? false);

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

    // 建立额外内容列表
    List<string> extl = [];
    Func<string> exnx = sharedConfig is null ?
        pbnx :
        () => {
            if (extl.Count > 0)
            {
                string f = extl[0];
                extl.RemoveAt(0);
                return f;
            }
            else
            {
                return pbnx.Invoke();
            }
        };

    // 构建主控制器
    Controller ctrl = new(player, exnx);

    // 将 PB 组件连接到控制器
    if (pb is not null)
    {
        ctrl.OnSec += pb.Inc_2;
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

    if (!devex)
    {
        io.WriteText("<Wraning> 指定的设备不存在\n");
    }

    // 获取额外内容
    if(sharedConfig is not null)
    {
        extl = sharedConfig.GetExtraContent();
    }

    // 启动
    ctrl.Start();
    if (httpController.listener.Prefixes.Count > 0)
    {
        httpController.Start();
        foreach (var pfxstr in httpController.listener.Prefixes)
        {
            io.WriteText($"HTTP RC URL : {pfxstr}");
        }
        io.WriteText("");
    }
    ConsoleHandler(ctrl.Pass, ctrl.Stop);
}