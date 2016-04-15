using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MyLogin
{
    public partial class MainWindow : Window
    {
        // TIP !
        // W metodach anonimowych jesli zrobimy cos takiego () => LoginAsync() to oznacza tyle co
        // () => {return LoginAsync;}

        // TIP 2
        // Metody asynchorniczne nie powinny zwracac VOID bo metoda ktora bedzie ja wywolywac nie bedzie miec informacji o EXCEPTION !

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            #region First Case - Block UI with .Wait()
            //var task = Task.Delay(1).ContinueWith((t) =>
            //            {
            //                Dispatcher.Invoke(() => { });
            //            });

            //task.Wait(); // DEADLOCK blokuje caller thread (w tym wypadku UI thread) az Task zostanie wykonany
            //             // czemu jest DEADLOCK ???
            //             // Bo .Wait() blokuje glowny thread UI a przeciez w Tasku chcemy cos zrobic na tym watku !!! :):) 
            #endregion

            #region Second Case - waiting for result
            // Probujac wyciagnac Result z LoginAsync() blokujemy UI Thread nie pozwalajac wykonac sie tej metodzie
            // chcemy wczesniej znac result zanim sie wykona blokujac tym samym UI THread ktory uruchamia task

            // Rozumiem to tak ze podczas LoginAsnc().Result chcemy wywolac na glownym watku Pobrac wartosc z LoginAsync blokujac UI thread
            // W rozwiazaniu z taskiem, mowimy ze - > czekam na result az zostanie wykonany na innym watku. Wczesniej ten watek uruchamiam
            // w pierwsszym przykladzie watek nie zostal uruchomiony bo glowny watek go zablokowal.

            //var result =  LoginAsync().Result;  // LOginAsync jest uruchamiany (tzn chce byc uruchomiony) na glownym watku

            //State machine jest blokowan przez .Result !!!

            // Jak to naprawic?
            //Bloujemy watek UI do momentu az wzynik z Task (state machine) bedzie dostepny
            var result = Task.Run( () =>
               { return LoginAsync(); }  // musi byc return :)
            ).Result;

            #endregion
        }

        private async Task<string> LoginAsync()
        {
            var loginTask = await Task.Run(() => {
                Thread.Sleep(2000);
                return "Login successful!";

            });

            return  loginTask;
        }

        private void ShowLoginBusyIndicator(bool show)
        {
            if (show)
            {
                BusyIndicator.IsEnabled = false;
                BusyIndicator.Visibility = Visibility.Visible;
            }
            else
            {
                BusyIndicator.IsEnabled = true;
                BusyIndicator.Visibility = Visibility.Hidden;
            }
        }
    }
}
