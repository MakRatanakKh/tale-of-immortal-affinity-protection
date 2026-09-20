using System;
using MOD_Rk7Qp2;

int checks = 0;
void Check(string name, float current, float requested, float expected)
{
    float actual = AffinityCapPolicy.Apply(current, requested);
    if (actual != expected)
        throw new Exception($"FAIL {name}: current={current}, requested={requested}, expected={expected}, actual={actual}");
    Console.WriteLine("PASS " + name);
    checks++;
}

Check("prevent ordinary loss", 250f, 245f, 250f);
Check("allow ordinary gain", 250f, 260f, 260f);
Check("preserve fractional floor", 299.5f, 299f, 299.5f);
Check("allow fractional gain", 299.125f, 299.5f, 299.5f);
Check("cap fractional gain", 299.5f, 305f, 300f);
Check("allow exact cap", 299.5f, 300f, 300f);
Check("normalize overcap to 300", 303.375f, 300f, 300f);
Check("normalize overcap even for lower request", 303.375f, 299.25f, 300f);
Check("normalize overcap on attempted further gain", 303.375f, 309f, 300f);
Check("normalize overcap for intermediate value", 303.375f, 302f, 300f);
Check("leave unrelated low value handling to filter", -60f, -50f, -50f);
Check("preserve negative affinity floor", -60.5f, -61f, -60.5f);
if (!float.IsNaN(AffinityCapPolicy.Apply(250f, float.NaN)))
    throw new Exception("FAIL exceptional input");
Console.WriteLine("PASS exceptional input");
checks++;
Console.WriteLine($"All {checks} affinity cap policy checks passed.");
