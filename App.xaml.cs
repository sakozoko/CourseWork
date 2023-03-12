using System.ComponentModel;
using System.Threading;
using System.Windows;
using CourseWork.Models;
using CourseWork.Services;
using CourseWork.Views;

namespace CourseWork;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly ILogger _logger;
    private readonly IHasher Hasher;
    private readonly ITokenCreator TokenCreator;
    private readonly IEncryptHandler EncryptHandler;
    private readonly ITokenHandler TokenHandler;
    private readonly AuthenticationService _authenticationService;
    private readonly AuthorizationService _authorizationService;
        public static readonly BackgroundWorker Worker = new();
        public App()
        {
            _logger = new FileLogger();
            Hasher = new Hasher();
            TokenCreator = new TokenCreator();
            EncryptHandler = new RsaEncryptHandler();
            TokenHandler = new TokenHandler(TokenCreator, EncryptHandler);
            _authenticationService = new AuthenticationService(TokenHandler, Hasher);
            _authorizationService = new AuthorizationService(_authenticationService, TokenHandler);
            if(!Worker.IsBusy)
            {
                Application.Current.Exit += (o, e) => Worker.CancelAsync();
                WorkerSetting();
            }
        }
        public void AppStartup(object sender, StartupEventArgs e){
            var mainWindow = new MainWindow(_authenticationService, _authorizationService,_logger);
            mainWindow.Show();
        }
        private void WorkerSetting()
    {
        Worker.WorkerSupportsCancellation = true;
        Worker.WorkerReportsProgress = true;
        Worker.DoWork += (obj, e) =>
        {
            var worker = (BackgroundWorker)obj!;
            while (!worker.CancellationPending)
            {
                if(Client.Token is null)
                continue;
                if (_authenticationService.IsTokenExpired(Client.Token!))
                {
                    var result = MessageBox.Show("Token expired, renew it?", "Token expired",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result.Equals(MessageBoxResult.Yes))
                    {
                        var newToken = _authenticationService.RenewToken(Client.Token);
                        if (newToken is not null)
                        {
                            var user = _authenticationService.GetUser(newToken);
                            Client.Token = newToken;
                            worker.ReportProgress(0, new WorkerState(){User = user, WorkerResult=WorkerResult.TokenRenewed});
                        }
                        else
                        {
                            Client.Token = null;
                            worker.ReportProgress(0,new WorkerState(){WorkerResult = WorkerResult.CannotRenewed});
                        }
                           
                    }
                    else
                    {
                        Client.Token = null;
                        worker.ReportProgress(0, new WorkerState(){WorkerResult = WorkerResult.TokenExpired});
                    }
                }
                Thread.Sleep(100);
            }
        };
        Worker.RunWorkerAsync();
    }
}