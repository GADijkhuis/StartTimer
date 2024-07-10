namespace StartTimer;

using System.Diagnostics;
using Microsoft.Extensions.Options;
using Plugin.Maui.Audio;

public partial class MainPage : ContentPage
{
    DateTime endTime;
    DateTime pausedTime;
    TimeSpan timePaused;
    IDispatcherTimer baseTimer;

    IAudioManager audioManager;
    IAudioPlayer audioPlayer;
    IAudioPlayer audioPlayerLong;
    bool timerRunning = false;
    bool timerPaused = false;
    int startMinutes;
    int countDownSeconds;
    bool countDownEnabled;
    bool countDown10secEnabled;
    List<int> warningMinutes = new List<int>();

    public MainPage()
    {
        InitializeComponent();

        audioManager = AudioManager.Current;

        baseTimer = Dispatcher.CreateTimer();
        baseTimer.Interval = TimeSpan.FromSeconds(1);
        baseTimer.Tick += updateTime;

        this.NavigatedTo += delegate
        {
            create();
        };

        create();

        DeviceDisplay.Current.KeepScreenOn = true;
    }

    public async void create()
    {
        audioPlayer = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("beep.wav"));
        audioPlayerLong = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("beepLong.wav"));

        startMinutes = Preferences.Get("StartMinutes", 5);
        countDownEnabled = Preferences.Get("CountDown", true);
        countDownSeconds = Preferences.Get("CountDownSeconds", 15);
        countDown10secEnabled = Preferences.Get("10secCountDown", true);
        string warningMinutesToConvert = Preferences.Get("WarningMinutes", "1;2;3;4;5");

        warningMinutes.Clear();
        foreach (string s in warningMinutesToConvert.Split(';'))
        {
            if (int.TryParse(s, out int i))
            {
                warningMinutes.Add(i);
            }
        }

        string minuteText = startMinutes < 10 ? "0" + startMinutes.ToString() : startMinutes.ToString();
        LBL_time.Text = $" {minuteText}:00 ";
        LBL_time_minutes.Text = $"Tijdsduur: {startMinutes} minuten";

        LBL_countdown.IsVisible = true;

        if (countDownEnabled)
            LBL_countdown.Text = $"Aftellen vanaf: {countDownSeconds} seconden";
        else
            LBL_countdown.IsVisible = false;
    }

    private async void BTN_options_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new OptionsPage());
    }

    private void BTN_start_Clicked(object sender, EventArgs e)
    {
        if (!timerRunning)
        {
            BTN_start.Text = "Pauze";
            timerRunning = true;
            endTime = DateTime.Now.AddMinutes(startMinutes);
            createSound();
            BTN_options.IsEnabled = false;
            baseTimer.Start();
        }
        else if (!timerPaused)
        {
            BTN_start.Text = "Hervatten";
            baseTimer.Stop();
            timerPaused = true;
            pausedTime = DateTime.Now;
        }
        else
        {
            BTN_start.Text = "Pauze";
            timePaused = timePaused.Add(DateTime.Now - pausedTime);
            timerPaused = false;
            baseTimer.Start();
        }

        updateTime(null, null);
    }

    private async void BTN_stop_Clicked(object sender, EventArgs e)
    {
        if (await DisplayAlert("Timer stoppen?", "Weet u zeker dat u de timer wilt stoppen? Deze kan niet worden hervat.", "Timer stoppen", "Annuleren"))
        {
            timerRunning = false;
            timerPaused = false;
            baseTimer.Stop();
            BTN_start.Text = "Start";
            BTN_options.IsEnabled = true;
            string minuteText = startMinutes < 10 ? "0" + startMinutes.ToString() : startMinutes.ToString();
            LBL_time.Text = $" {minuteText}:00 ";
        }
    }


    public void updateTime(object sender, EventArgs e)
    {
        TimeSpan timeRemaining = endTime - (DateTime.Now - timePaused);

        LBL_time.Text = timeRemaining.ToString(@"mm\:ss");

        if (timeRemaining.TotalSeconds < 1)
        {
            baseTimer.Stop();
            createLongSound();

            timerRunning = false;
            timerPaused = false;
            BTN_start.Text = "Start";
            BTN_options.IsEnabled = true;

            string minuteText = startMinutes < 10 ? "0" + startMinutes.ToString() : startMinutes.ToString();

            LBL_time.Text = $" {minuteText}:00 ";
        }
        else if (countDown10secEnabled && timeRemaining.Minutes == 0 && int.TryParse(timeRemaining.TotalSeconds.ToString().Split(',')[0], out int secondsRemaining) && secondsRemaining % 10 == 0)
            createSound();
        else if (countDownEnabled && timeRemaining.TotalSeconds <= countDownSeconds + 1)
            createSound();
        else if (timeRemaining.Seconds == 0 && warningMinutes.Contains(timeRemaining.Minutes))
            createSound();


    }

    public void createSound()
    {
        audioPlayer.Play();
    }
    public void createLongSound()
    {
        audioPlayerLong.Play();
    }
}
