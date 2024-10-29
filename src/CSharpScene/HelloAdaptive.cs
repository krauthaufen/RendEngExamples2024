using CSharp.Data.Adaptive;
using FSharp.Data.Adaptive;

namespace PlainAardvarkRendering_NetFramework;

public class HelloAdaptive
{
    public static void Run()
    {
        var a = new ChangeableValue<int>(1);
        var b = new ChangeableValue<int>(1);

        var c = a.Map(va => va * 2);
        var d = b.Map(c, (vb, vc) => vb + vc);
        Console.WriteLine("D: {0}", d.GetValue());

        using (Adaptive.Transact) { a.Value = 3; }
        Console.WriteLine("D: {0}", d.GetValue());
        
    }
}