using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;


namespace netCoreUsersAPI.Controllers
{

    [Route("api/[controller]/random")]
    [ApiController]
    public class UserController : ControllerBase
    {
        IDistributedCache _memoryCache;

        public UserController(IDistributedCache memoryCache)
        {
            _memoryCache = memoryCache;
        }


        string[] firstNames = new string[] { "Adam", "Alex", "Aaron", "Ben", "Carl", "Dan", "David", "Edward", "Fred", "Frank", "George", "Hal", "Hank", "Ike", "John", "Jack", "Joe", "Larry", "Monte", "Matthew", "Mark", "Nathan", "Otto", "Paul", "Peter", "Roger", "Roger", "Steve", "Thomas", "Tim", "Ty", "Victor", "Walter" };

        string[] lastNames = new string[] { "Anderson", "Ashwoon", "Aikin", "Bateman", "Bongard", "Bowers", "Boyd", "Cannon", "Cast", "Deitz", "Dewalt", "Ebner", "Frick", "Hancock", "Haworth", "Hesch", "Hoffman", "Kassing", "Knutson", "Lawless", "Lawicki", "Mccord", "McCormack", "Miller", "Myers", "Nugent", "Ortiz", "Orwig", "Ory", "Paiser", "Pak", "Pettigrew", "Quinn", "Quizoz", "Ramachandran", "Resnick", "Sagar", "Schickowski", "Schiebel", "Sellon", "Severson", "Shaffer", "Solberg", "Soloman", "Sonderling", "Soukup", "Soulis", "Stahl", "Sweeney", "Tandy", "Trebil", "Trusela", "Trussel", "Turco", "Uddin", "Uflan", "Ulrich", "Upson", "Vader", "Vail", "Valente", "Van Zandt", "Vanderpoel", "Ventotla", "Vogal", "Wagle", "Wagner", "Wakefield", "Weinstein", "Weiss", "Woo", "Yang", "Yates", "Yocum", "Zeaser", "Zeller", "Ziegler", "Bauer", "Baxster", "Casal", "Cataldi", "Caswell", "Celedon", "Chambers", "Chapman", "Christensen", "Darnell", "Davidson", "Davis", "DeLorenzo", "Dinkins", "Doran", "Dugelman", "Dugan", "Duffman", "Eastman", "Ferro", "Ferry", "Fletcher", "Fietzer", "Hylan", "Hydinger", "Illingsworth", "Ingram", "Irwin", "Jagtap", "Jenson", "Johnson", "Johnsen", "Jones", "Jurgenson", "Kalleg", "Kaskel", "Keller", "Leisinger", "LePage", "Lewis", "Linde", "Lulloff", "Maki", "Martin", "McGinnis", "Mills", "Moody", "Moore", "Napier", "Nelson", "Norquist", "Nuttle", "Olson", "Ostrander", "Reamer", "Reardon", "Reyes", "Rice", "Ripka", "Roberts", "Rogers", "Root", "Sandstrom", "Sawyer", "Schlicht", "Schmitt", "Schwager", "Schutz", "Schuster", "Tapia", "Thompson", "Tiernan", "Tisler" };

        private string RandomName()
        {
            Random r = new Random();
            return firstNames[r.Next(0, firstNames.Length)] + " " + lastNames[r.Next(0, firstNames.Length)];

           
        }

        private void SetCached()
        {
            if (GetCached() == null)
            {
                var Time = DateTime.Now.ToLocalTime().ToString();
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddSeconds(5)
                };
                _memoryCache.Set("Time", Encoding.UTF8.GetBytes(Time), cacheOptions);
            }
        }

        private bool LogAPIRequest(string sessionId, string path, int cooldownMins)
        {
          

            if (GetAPIRequest(sessionId, path) == null)
            {
                //no cache key - allow a new request
                //set cache key
                
                var Time = DateTime.Now.ToLocalTime().ToString();
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddSeconds(cooldownMins)
                };
                _memoryCache.Set(sessionId + "-" + path, Encoding.UTF8.GetBytes(Time), cacheOptions);
                return true;
            }
            else
            {
                //still needs to chill
                return false;
            }
        }

        private string GetAPIRequest(string sessionId, string path)
        {
            if (_memoryCache.Get(sessionId + "-" + path) != null)
            {
                string Time = string.Empty;
                Time = Encoding.UTF8.GetString(_memoryCache.Get(sessionId + "-" + path));
                return Time;
            }
            else
            {
                return null;
            }
        }

        private string GetCached()
        {
            if (_memoryCache.Get("Time") != null)
            {
                string Time = string.Empty;
                Time = Encoding.UTF8.GetString(_memoryCache.Get("Time"));
                return Time;
            }
            else
            {
                return null;
            }
        }




        // GET: api/<UserController>
        [HttpGet]

        public IEnumerable<User> Get()
        {
            //setup something in the session:

            HttpContext.Session.SetString("UserAPIkey", "fake_token_here");

            string sessId = HttpContext.Session.Id;
            bool allowAPIRequest = LogAPIRequest(sessId, "user/random", 5);


            string sid = HttpContext.Session.IsAvailable.ToString();

            if (allowAPIRequest)
            {
                var rng = new Random();
                return Enumerable.Range(1, 10).Select(index => new User
                {
                    createdBy = sessId,
                    createdOn = DateTime.Now,
                    id = rng.Next(1000000, 999999999),
                    name = RandomName(),
                    url = "https://avatars.dicebear.com/api/male/" + rng.Next(1, 10000) + ".svg"
                })
                .ToArray();
            }
            else
            {


                HttpContext.Response.StatusCode = 429;



                return null;


            }
        }






    }
}


