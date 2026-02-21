using System;
using System.Windows.Forms;

namespace StandaloneRunner;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new RunnerGameForm());
    }
}
