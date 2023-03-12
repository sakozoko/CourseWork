using System.ComponentModel;
using System.Windows;
using CourseWork.Models;
using CourseWork.Services;

namespace CourseWork.Views;

public partial class SecuredResourceWindow : Window
{
    private readonly AuthenticationService _authenticationService;
    private readonly ILogger _logger;
    private readonly AuthorizationService _authorizationService;

    public SecuredResourceWindow(AuthenticationService authenticationService,
     AuthorizationService authorizationService,
     ILogger logger)
    {
        _logger = logger;
        _authorizationService = authorizationService;
        _authenticationService = authenticationService;
        InitializeComponent();
        Init();
    }

    private void Init()
    {
        if(!_authorizationService.Authorize(Client.Token))
        {
            SecuredResource.Text = "You don't have access to this resource";
            return;
        }
        var user = _authenticationService.GetUser(Client.Token);
        if (user is null)
            SecuredResource.Text = "You don't have access to this resource";
        else{
SecuredResource.Text =
                $"Hello, {user.Username}! You have access to this resource, your access level is {user.AccessLevel}";
                TextBlock.Visibility = Visibility.Visible;
        }
             App.Worker.ProgressChanged += WorkerProgress;
        Closing += (o, e) => App.Worker.ProgressChanged -= WorkerProgress;
        
    }

       private void WorkerProgress(object? o, ProgressChangedEventArgs e)
    {
        var workerState = (WorkerState)e.UserState!;
        var window=this;
            Dispatcher.Invoke(() =>
            {
                switch (workerState.WorkerResult)
                {
                    case WorkerResult.CannotRenewed:
                        MessageBox.Show("Token expired, please login again", "Token expired",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        SecuredResource.Text = "Not logged in";
                        TextBlock.Visibility = Visibility.Collapsed;
                        break;
                    case WorkerResult.TokenRenewed:
                        SecuredResource.Text = $"Hello, {workerState.User?.Username}! You have access to this resource, your access level is {workerState.User?.AccessLevel}";
                        TextBlock.Visibility = Visibility.Visible;
                        break;
                    case WorkerResult.TokenExpired:
                        SecuredResource.Text = "Not logged in";
                        TextBlock.Visibility = Visibility.Collapsed;
                        break;
                }
            });
    }


    public void Logout_Click(object sender, RoutedEventArgs e)
    {
        _logger.Log($"User {_authenticationService.GetUser(Client.Token)?.Username} logged out");
        _authenticationService.Logout();
        var mainWindow = new MainWindow(_authenticationService, _authorizationService, _logger);
        mainWindow.Show();
        Close();
    }

    public void Back_Click(object sender, RoutedEventArgs e)
    {
        _logger.Log($"User {_authenticationService.GetUser(Client.Token)?.Username} went back to main window");
        var mainWindow = new MainWindow(_authenticationService  , _authorizationService, _logger);
        mainWindow.Show();
        Close();
    }
}