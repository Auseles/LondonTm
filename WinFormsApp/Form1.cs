using Newtonsoft.Json;
using System.Diagnostics;

namespace WinFormsApp
{
    public partial class Form1 : Form
    {
        public class LondonTime
        {
            public string abbreviation;
            public string datetime;
            public int day_of_week;
            public int day_of_year;
            public bool dst;
            public string dst_from;
            public int dst_offset;
            public string dst_until;
            public int raw_offset;
            public string timezone;
            public int unixtime;
            public string utc_datetime;
            public string utc_offset;
            public int week_number;
        }
        List<Task> tasks = new List<Task>();
        public static int timerTicks = 0;
        public static int succsessTicks = 0;
        public static Form1 context;
        public static object locker = new object();
        private static HttpClient sharedClient = new()
        {
            BaseAddress = new Uri("http://worldtimeapi.org/api/timezone/Europe/London"),
        };

        public static Stopwatch watch;
        public Form1()
        {
            InitializeComponent();
            context = this;
            watch = Stopwatch.StartNew();
            TimerCallback tm = new TimerCallback(TimerTick);
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(new Task(() => taskBody()));
            }
            System.Threading.Timer timer = new System.Threading.Timer(tm, null, 0, 500);
        }
        private void TimerTick(object state)
        {
            timerTicks++;
            int tasksWaitsStarts = (timerTicks - succsessTicks) * 2;
            int tasksRuns = 0;
            for (int i = 0; i < 10; i++)
            {
                if (tasks[i].IsCompleted == false && tasks[i].Status == TaskStatus.Created && tasksRuns < tasksWaitsStarts)
                {
                    tasks[i].Start();
                    tasksRuns++;
                }
                if (tasks[i].Status == TaskStatus.Faulted || tasks[i].IsCompleted == true)
                {
                    tasks[i] = new Task(() => taskBody());
                }
            }
        }
        public static void taskBody()
        {
            try
            {
                lock (locker)
                {
                    var rest = sharedClient.GetAsync(sharedClient.BaseAddress).Result;
                    string json = rest.Content.ReadAsStringAsync().Result;
                    LondonTime deserialized = JsonConvert.DeserializeObject<LondonTime>(json);
                    context.label1.Invoke((Action)delegate { context.label1.Text = deserialized.datetime; });
                    succsessTicks++;
                }
            }
            catch (HttpRequestException ex)
            {
                context.label1.Invoke((Action)delegate { "Ошибка подключения".ToString(); });
            }
        }
    }
}
