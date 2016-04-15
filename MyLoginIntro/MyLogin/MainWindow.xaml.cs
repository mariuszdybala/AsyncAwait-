using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MyLogin
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }

        // Wazna roznica miedzy AsyncAwait a ContinueWith !
        // W ContinueWith kontynuacja odbywa sie na INNYM watku niz UI !
        // W przypadku AsyncAwait kontynuacja jest na watku UI Thread (calling context - > UI thread ) !!

        // Wszystko co znajduje sie po metodzie z await - > jest wykonywane jako kontyuacja !
        // uzywaj trycatch !
        // 
        #region AsyncAwait
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            #region First Aproach Without external method
            //// dodanie await sprawi ze LoginButton.Content = "Login successful !"; jest wykonywane po Task'u
            //// z await zwracany jest result = > nie Task
            //var result = await Task.Run(() =>
            //{
            //    Thread.Sleep(2000);
            //    return "Login successful";
            //});

            //// LoginButton.Content = "Login successful !";
            //LoginButton.Content = result; 
            #endregion

            #region Second Aproach with External Method

            try
            {
                BusyIndicator.IsEnabled = false;
                BusyIndicator.Visibility = Visibility.Visible;


                var result = await LoginAsync();
                //var result = await LoginAsync().ConfigureAwait(false);  // HA HA ! uzywajac ConfigureAwait chcemy aby kolejna operacja
                // byla rowniez wykonywana na tymsamym pobocznym watku !
                // a przeciez chcemy wykonac cos na UI watku ! - > i.e LoginButton.Content = result;

                LoginButton.Content = result;

                BusyIndicator.IsEnabled = true;
                BusyIndicator.Visibility = Visibility.Hidden;
            }
            catch (Exception)
            {
                LoginButton.Content = "Internal Error !";
            }

            #endregion
        }

        #region LoginAsync with value return

        private async Task<string> LoginAsync()
        {
            try
            {
                #region Wait for all TASK will be excecuted
                //var result = await Task.Run(() =>
                //    {
                //        Thread.Sleep(2000);
                //        return "Login successful";
                //    });
                ////BACK to UI
                //await Task.Delay(1000); // symuluje koljeny task cos jak Thread.Sleep()
                //                        //BACK to UI
                //await Task.Delay(1000); // operacja 2
                //                        //BACK to UI
                //await Task.Delay(1000); //operacja 1 etc...
                //                        //BACK to UI
                //                        // Mozna tylko awaitow zwrobic ile sie chce, kazdy moze wykonywac inna operacje ;)

                //return result;
                #endregion

                // Czekam na wszystkie taski. Ale wczesniej musze je zadeklarowac bezposrednio.
                #region Finish with Task.WhenAll 
                var loginTask = Task.Run(() =>
                    {
                        Thread.Sleep(2000);
                        return "Login successful";
                    });
                //BACK to UI
                var logTask = Task.Delay(1000); // symuluje koljeny task cos jak Thread.Sleep()
                                        //BACK to UI
                var DoSomething = Task.Delay(1000); // operacja 2
                                        //BACK to UI
                var DoSomethingElse = Task.Delay(1000); //operacja 1 etc...
                                                        //BACK to UI
                                                        // Mozna tylko awaitow zwrobic ile sie chce, kazdy moze wykonywac inna operacje ;)

                await Task.WhenAll(loginTask, logTask, DoSomething, DoSomethingElse);

                return loginTask.Result;
                #endregion

            }
            catch (Exception)
            {
                return "Login Failed !";
            }
        }

        #endregion

        #region LoginAsync with void return
        // Musimy dodac zamiast void - > Task  . Wtedy compiler moze sobie wyciagnac stacktrace do exception  i przekazac dalej
        // jesli zwracamy void - > kompiler nic nie wie o Exception...
        //private async Task LoginAsync()
        //{
        //    try
        //    {
        //        //Exception zostanie wywalapny LoginButton_Click handler i w catchu zostanie ustawione LoginButton.Content = "Internal Error !";
        //        //throw new UnauthorizedAccessException(); // niew wiem czemu ciagle wywala mi blad przy exceptions
        //        var result = await Task.Run(() =>
        //          {
        //              // wewnatrz Taska tez mozemy wywalic Exception, ale zostanie wylapany przez catcha w LoginAsync
        //              //throw new UnauthorizedAccessException(); // niew wiem czemu ciagle wywala mi blad przy exceptions
        //              Thread.Sleep(2000);
        //              return "Login successful";
        //          });

        //        LoginButton.Content = result;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //} 
        #endregion

        #endregion

        // Wazna uwaga. W przypadku Task (bez async/await) jesli chcemy wykonujemy cos po tasku to jest ono wykonywane od razu
        // .Net nie czeka na wykonanie Tasku zeby zrobic operacja np. LogginButton.Content = "Test"
        // Aby poczekac na wykonanie taska nalezy skorzystac z dwoch sposobow ContinueWith lub COnfigureAwait
        // Ale najlepiej skorzystac z AsyncAwait ( sposob u gory)

        #region Asynchronous Explaination
        //private void LoginButton_Click(object sender, RoutedEventArgs e)
        //{
        //    LoginButton.IsEnabled = false;
        //    var task = Task.Run(() =>
        //    {
        //        throw new UnauthorizedAccessException();  // nie wiem czemu wysypywal mi sie program jak wyrzucal wyjatek
        //        //ale powinno byc tak ze Task zwraca wtedy do ContineWith property IsFoult = true
        //        Thread.Sleep(2000);
        //        return "Login successfull";
        //    }
        //    );

        //    //LoginButton.Content = "Test - wykonywany od razu nie czekajac na Task";

        //    #region Continuation with ConfigureAwait()
        //    //task.ConfigureAwait(true).GetAwaiter().OnCompleted(() =>
        //    //{
        //    //    LoginButton.Content = task.Result;
        //    //    LoginButton.IsEnabled = true;
        //    //}); // jesli podajemy true, to wracamy do glownego watka
        //    //    // GetAwaiter czeka na wykonanie Taska (mamy mozeliwosc sprawdzenia przez property IsCompleted)
        //    //    // lub wykorzystac metode OnComplete i w niej wykonac juz operacje na glownym watku
        //    //    // bez korzystania z Dispatchera (bo jestesmy juz na UI'u)
        //    //    // ConfigureAwait nazwa metody sama nasuwa ze czekamy na wykonanie danego Taska 
        //    #endregion

        //    #region Continuation with ContinueWith
        //    task.ContinueWith((t) =>
        //    {

        //        if (t.IsFaulted)
        //        {
        //            Dispatcher.Invoke(() =>
        //            {
        //                LoginButton.Content = "Login Failed";
        //                LoginButton.IsEnabled = true;

        //            });
        //        }
        //        Dispatcher.Invoke(() =>
        //        {
        //            LoginButton.Content = t.Result;
        //            LoginButton.IsEnabled = true;
        //        });
        //    });
        //    #endregion
        //}

        #endregion
    }
}
