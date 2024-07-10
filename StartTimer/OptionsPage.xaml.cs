using System.Diagnostics;

namespace StartTimer;

public partial class OptionsPage : ContentPage
{

    public OptionsPage()
    {
        InitializeComponent();

        this.NavigatedFrom += delegate { save(); };

        create();
    }
    public void create()
    {
        CB_countDown.CheckedChanged += delegate {
            TB_countDownSeconds.IsEnabled = CB_countDown.IsChecked;
        };

        TB_startMinutes.Text = Preferences.Get("StartMinutes", 5).ToString();
        CB_countDown.IsChecked = Preferences.Get("CountDown", true);
        TB_countDownSeconds.Text = Preferences.Get("CountDownSeconds", 15).ToString();
        CB_10secCountDown.IsChecked = Preferences.Get("10secCountDown", true);
        string warningMinutesToConvert = Preferences.Get("WarningMinutes", "1;2;3;4;5");

        LYT_warningMinutes.Children.Clear();
        foreach(string s in warningMinutesToConvert.Split(';')){
            if (int.TryParse(s, out int i)) {
                addWarningMinuteToLayout(i);
            }
        }
    }
    private void BTN_removeWarningMinute_Clicked(object sender, EventArgs e)
    {
        if (LYT_warningMinutes.Children.Count <= 0) {
            return;
        }
        LYT_warningMinutes.Children.RemoveAt(LYT_warningMinutes.Children.Count - 1);
    }
    private void BTN_addWarningMinute_Clicked(object sender, EventArgs e)
    {
       addWarningMinuteToLayout();
    }
    private void addWarningMinuteToLayout(int minutes = 0)
    {
        Entry TB_warningMinutes = new Entry(){
            Text = minutes == 0 ? "" : minutes.ToString(),
            Keyboard = Keyboard.Numeric
        };
        
        TB_warningMinutes.TextChanged += validateEntryInput;

        LYT_warningMinutes.Children.Add(TB_warningMinutes);
    }
    private async void validateEntryInput(object sender, TextChangedEventArgs e)
    {
        if ((sender as Entry).Text != "" && (!int.TryParse((sender as Entry).Text, out int i) || i < 0)) {
            (sender as Entry).Text = "1";
            await DisplayAlert("Ongeldige invoer", "Invoer moet een getal zijn en groter zijn dan 0", "OK");
        }
    }

    public void save()
    {
        Preferences.Set("StartMinutes", int.TryParse(TB_startMinutes.Text, out int StartOutput) ? StartOutput : 5);
        Preferences.Set("CountDown", CB_countDown.IsChecked);
        Preferences.Set("CountDownSeconds", int.TryParse(TB_countDownSeconds.Text, out int CountDownOutput) ? CountDownOutput : 15);
        Preferences.Set("10secCountDown", CB_10secCountDown.IsChecked);

    
        string warningMinutesToSave = "";
        foreach (object o in LYT_warningMinutes.Children) {
            if (o.GetType() != typeof(Entry) || !int.TryParse((o as Entry).Text, out _))
                continue;
            
            warningMinutesToSave += $"{(o as Entry).Text};";
        }
        Debug.WriteLine(warningMinutesToSave);
        Debug.WriteLine(CB_countDown.IsChecked);
        Preferences.Set("WarningMinutes", warningMinutesToSave);
    }
}
