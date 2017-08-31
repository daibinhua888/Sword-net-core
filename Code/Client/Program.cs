using IService;
using Sword.Clients;
using System;
using System.Threading;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Client1";

            Thread.Sleep(2000);

            SwordConfiguration.SetServerInfo("localhost", 888);

            while (true)
            {
                using (var proxy = new Sword<ITest>())
                {
                    for (var i = 0; i < 500; i++)
                    {
                        try
                        {
                            var result = proxy.Proxy.Test1("fff");
                            proxy.Proxy.Test2();

                            Console.WriteLine(i + "====" + result.P1);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("server exception: ");
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                Console.WriteLine("============================");
                //Thread.Sleep(500);

                using (var proxy = new Sword<ITest2>())
                {
                    for (var i = 0; i < 500; i++)
                    {
                        var result = proxy.Proxy.Test2("fff");

                        Console.WriteLine(i + "====" + result);
                    }
                }

                for (var i = 0; i < 500; i++)
                {
                    using (var proxy = new Sword<ITest3>())
                    {
                        try
                        {
                            var result1 = proxy.Proxy.Test3_1("fff");

                            Console.WriteLine(result1);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("server exception: ");
                            Console.WriteLine(ex.Message);
                        }

                        try
                        {
                            var result2 = proxy.Proxy.Test3_2("fff");

                            Console.WriteLine(result2);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("server exception: ");
                            Console.WriteLine(ex.Message);
                        }

                        try
                        {
                            proxy.Proxy.Test3_3("fff");

                            Console.WriteLine("executed");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("server exception: ");
                            Console.WriteLine(ex.Message);
                        }
                    }
                }

                Console.WriteLine("*************************");
            }
        }
    }
}
