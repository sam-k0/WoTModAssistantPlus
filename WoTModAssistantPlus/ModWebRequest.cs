using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;

namespace WoTModAssistant
{
    public class ModWebRequest
    {
        private const string API_STARTPAGE_BASE_QUERY = "https://wgmods.net/api/mods/mods_start_page/";

        public async Task<string> GetModsByKeyword(string keyword, string gameversion="167") // Current gameversion is 167
        {
            var client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync("https://wgmods.net/api/mods/mods_start_page/?language=en&limit_recommended=12&limit_new=4&limit_updated=4&game_version_id=167");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return "-1";
            }
        }

        public async Task<string> GetRecommendedMods( string gameversion = "167", int limit = 12) // Current gameversion is 167
        {
            var client = new HttpClient();
            try
            {
                string url = API_STARTPAGE_BASE_QUERY + "?language=en&limit_recommended="+limit.ToString()+"&limit_new=1&limit_updated=1&game_version_id=" + gameversion;
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
               
                return "-1";
            }
        }
    } 
}
