using System;
using System.Runtime.Serialization.Formatters.Binary;
using static System.Console;

namespace Airplane_Pilot_Simulator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Title = "Тренажер пилота самолета\n(Автор: Анна Ерошина)";
            
            CursorVisible = false;
            BackgroundColor = ConsoleColor.DarkBlue;

            Display.Greeting();
            WriteLine("\n\t\tЕсли готовы, нажмите 1");
            try
            {
                int pos = Convert.ToInt32(ReadLine());
                if (pos == 1)
                {
                    Clear();
                    // В процессе тренировки пилотов самолета используется только один объект самолета
                    Plane plane = new Plane();
                    //Присваиваем имена диспетчерам с клавиатуры
                    plane.AddDispatchers();
                    //Запуск полета
                    plane.Fly(plane);

                }
                else
                    return;
            }
            catch (AirplaneCrushed ac) //если разбился
            {
                SetCursorPosition(30, 10);
                WriteLine(ac.Message);
                Beep();
                Plane p = null;
                BinaryFormatter bf = new BinaryFormatter();
                Plane.BlackBox();

            }
            catch (Unsuitable u) //если не годен
            {
                SetCursorPosition(10, 5);
                WriteLine(u.Message);
                Beep();
                Plane.BlackBox();
            }
            catch (Exception ex)
            {
                SetCursorPosition(10, 5);
                WriteLine(ex.Message);
            }

        }
    }
}

