using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace YDKK.Client
{
    public class Jobcan
    {
        public enum Status
        {
            HavingBreakfast,
            Resting,
            ReturnedHome,
            Working,
            UnKnown
        }

        private readonly Dictionary<string, Status> _statusDict = new Dictionary<string, Status>
        {
            { "having_breakfast", Status.HavingBreakfast },
            { "resting", Status.Resting },
            { "returned_home", Status.ReturnedHome },
            { "working", Status.Working }
        };

        private readonly string _clientID;
        private readonly string _email;
        private readonly string _password;
        private readonly HttpClient _client;
        private readonly Timer _timer = new Timer(1000 * 60 * 10); //10min

        private Jobcan(string clientID, string email, string password)
        {
            _clientID = clientID;
            _email = email;
            _password = password;
            _client = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = false
            })
            {
                BaseAddress = new Uri("https://ssl.jobcan.jp/")
            };
            _client.DefaultRequestHeaders.Add("User-Agent", "YDKK.Client.Jobcan");
        }

        public static async Task<Jobcan> LoginAsync(string clientID, string email, string password, bool keepSession = false)
        {
            var jobcan = new Jobcan(clientID, email, password);

            if (!await jobcan.Login())
            {
                throw new Exception("Failed to login.");
            }

            if (keepSession)
            {
                jobcan._timer.Elapsed += async (sender, args) =>
                {
                    await jobcan._client.PostAsync("employee/index/noop", new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded"));
                };
                jobcan._timer.Start();
            }

            return jobcan;
        }

        public async Task<bool> Login()
        {
            var payload = $"client_id={Uri.EscapeDataString(_clientID)}&email={Uri.EscapeDataString(_email)}&password={Uri.EscapeDataString(_password)}&login_type=1";
            var contents = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded");
            var result = await _client.PostAsync("login/pc-employee", contents);
            return (int)result.StatusCode >= 300 && (int)result.StatusCode <= 399 && !result.Headers.Location.OriginalString.EndsWith("error");
        }

        public async Task<Status> GetStatusAsync()
        {
            var r = new Regex(@"var current_status = ""(?<status>[a-z_]*?)"";");
            var result = await _client.GetAsync("employee");
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Failed to get status. Session may have expired.");
            }

            var content = await result.Content.ReadAsStringAsync();

            var match = r.Match(content);
            if (match.Success)
            {
                var statusStr = match.Groups["status"].Value;
                if (_statusDict.ContainsKey(statusStr))
                {
                    return _statusDict[statusStr];
                }
            }

            return Status.UnKnown;
        }
    }
}