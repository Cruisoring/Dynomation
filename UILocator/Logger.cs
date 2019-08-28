using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace UILocator
{
    public static class Logger
    {
        static List<Action<String>> Outputs;

//        private static readonly Process Proc;

        static Logger()
        {
            Outputs = new List<Action<string>>();
//            Outputs.Add(message => Console.Error.WriteLine(message));
            Outputs.Add(message => Trace.WriteLine(message));
            Outputs.Add(Console.WriteLine);
        }

        public static string TryFormat(string format, params object[] args)
        {
            String message;
            try
            {
                message = String.Format(format, args);
            }
            catch (Exception)
            {
                message = String.Format("format={0}, args={1}", format ?? "null", String.Join(", ", args));
            }

            return message;
        }

        static void Log(string format, params object[] args)
        {
            if(Outputs == null)
                return;

            String message = TryFormat(format, args);

            foreach (var output in Outputs)
            {
                try
                {
                        output.Invoke(message);
                }
                catch (Exception)
                {}
            }
        }

        public static void V(string format, params object[] args)
        {
            Log(DateTime.Now.ToString("hh:mm:ss")  + " [V] " + format, args);
        }

        public static void D(string format, params object[] args)
        {
            Log(DateTime.Now.ToString("hh:mm:ss")  + " [D] "  + format, args);
        }

        public static void I(string format, params object[] args)
        {
            Log(DateTime.Now.ToString("hh:mm:ss")  + " [I] "  + format, args);
        }

        public static void W(string format, params object[] args)
        {
            Log(DateTime.Now.ToString("hh:mm:ss")  + " [W] "  + format, args);
        }

        public static void E(string format, params object[] args)
        {
            Log(DateTime.Now.ToString("hh:mm:ss")  + " [E] "  + format, args);
        }

        public static void E(Exception ex)
        {
            Log(DateTime.Now.ToString("hh:mm:ss")  + " [E] " + ex.GetType() + " " + ":  {0}", ex.StackTrace);
        }
    }

}
