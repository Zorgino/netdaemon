using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.Common.Reactive;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Reactive.Linq;
using System.Runtime.Serialization;
// using Netdaemon.Generated.Extensions;
public class GlobalApp : NetDaemonRxApp
{
    // private ISchedulerResult _schedulerResult;
    private int numberOfRuns = 0;
    IDisposable task;
    //public string? SharedThing { get; set; }
    public override Task InitializeAsync()
    {
        //StateChanges.Select(f => f.New.EntityId=="light.tomas_rum" && 
        //                        f.New.State == "off")
        //                        //f.New.Attribute.brightness < 20)
        //                        .DistinctUntilChanged()
        //                        .Where(x => x)
        //                        .Throttle(TimeSpan.FromSeconds(10)).Subscribe(e => Log("Now!"));
        // Delay(TimeSpan.FromSeconds(5)).Where(e => DateTime.Now.Subtract(State("binary_sensor.tomas_rum_pir").LastChanged) >= TimeSpan.FromSeconds(5))
        //.Catch(ex => Log(ex, "ERROR IN THE STUFF"))
        task = StateChanges.Where(n => n.New.EntityId == "binary_sensor.tomas_rum_pir" && n.New.State == "off" && n.Old.State == "on")
            .Timeout(TimeSpan.FromSeconds(5)).Subscribe(e =>
        //StateChanges.Where(n => n.New.EntityId == "light.tomas_rum" && n.New.State == "off" ).Delay(TimeSpan.FromSeconds(10)).Where(e => DateTime.Now.Subtract(e.New.LastChanged) >= TimeSpan.FromSeconds(10)).Subscribe(e =>
        {
            Log("New changed:{changed}", e.New.LastChanged);
            //throw new Exception("helle");
        }); //, ex => Log(ex, "ERROR IN THE STUFF")
        //.Timeout(TimeSpan.FromSeconds(5)
        return Task.CompletedTask;
        //throw new Exception("Hello");
        //RunDaily("20:28:00").Subscribe(e => Log("Now!"));

        //Log("Rx - TEST");
        //States.Subscribe(t => { Log("{entity}: {state}({oldstate})", t.New.EntityId, t.New.State, t.Old.State); });

        //Log("Rx - TEST");
        //States.Where(t => t.New.EntityId.StartsWith("sensor.") && t.New.State != t.Old.State)
        //    .Subscribe(t => { Log("{entity}: {state}({oldstate})", t.New.EntityId, t.New.State, t.Old.State); });

        //int level = 10;

        //Observable.Interval(TimeSpan.FromSeconds(5)).Subscribe(e =>
        //{

        //    Entity("light.tomas_rum").TurnOn(new { brightness= level });

        //    var x = State("light.tomas_rum");
        //    Log("The light in Tomas room is : {state}", x?.State);
        //    SetState("sensor.works", "on", new { time = DateTime.Now.ToString() });
        //    Entity("sensor.test").SetState("on");
        //    level += 10;
        //});

        //StateChanges.Subscribe(t => 
        //{ 
        //    Log("{entity}: {state}({oldstate})", t.New.EntityId, t.New.State, t.Old.State);
        //    Thread.Sleep(1000);
        //    Log("Slept 1000 ms");
        //});
        //});
        //States.Subscribe(tuple =>
        //{
        //    var (o, n) = tuple;
        //    Log("{entity}: {state}({oldstate})", n.EntityId, n.State, o.State);
        //    Thread.Sleep(1000);
        //    Log("Slept 1000 ms");
        //});

        //this.Where( n => n.EntityId.StartsWith("light.")).Subscribe(x => Log(x.EntityId));
        //State.Where(x => x.EntityId.StartsWith("light.")).Subscribe(e =>
        //{
        //    Log("Rx - got global event for {0}, state is {1}", e.EntityId, e.State);
        //});

        //SharedThing = "Hello world";
        //Log("Logging from global app");
        //LogError("OMG SOMETING IS WRONG {error}", "The error!");

        //Entities(p =>
        //    {
        //        // await Task.Delay(5000);
        //        Thread.Sleep(10);
        //        return false;
        //    }).WhenStateChange("on", "off")
        //    .Call((a, b, c) => { Log("Logging from global app"); return Task.CompletedTask; })
        //    .Execute();

        //Log("AfterExecute");
        //// Entity("light.my_light").TurnOn();
    }
}


