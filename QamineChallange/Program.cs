using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace QamineChallange
{
    class Program
    {
        private static readonly Uri Url = new Uri("http://engineer.qamine.com");
        private const string Challenge = "challenge";
        private const string Answer = "answer";


        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var challangeString = GetChallangeString();
            var challange = ParseChallengeData(challangeString);
            var response = GetSubmitChallande(challange);
            Console.WriteLine(response);
            Console.WriteLine(stopwatch.Elapsed);
            Console.ReadLine();
        }

        private static string GetChallangeString()
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(Url, Challenge));
            var chanllangeResponse = (HttpWebResponse)request.GetResponse();
            using (var responseStream = chanllangeResponse.GetResponseStream())
            {
                using (var streamReader = new StreamReader(responseStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        private static ChallengeData ParseChallengeData(string challengeString)
        {
            var startIndex = challengeString.IndexOf("<pre>") + 5;
            var endIndex = challengeString.IndexOf("\n", startIndex);
            var preString = challengeString.Substring(startIndex, endIndex - startIndex);

            Console.WriteLine(preString);
            var data = new ChallengeData();

            if (preString.Contains("just subtract"))
            {
                data.Operator = ChallengeOperation.Subtract;
            } else if (preString.Contains("just add"))
            {
                data.Operator = ChallengeOperation.Add;
            }
            else
            {
                throw  new Exception("Invalid operator: " + preString);
            }

            var matches = Regex.Matches(preString, @"\d+");
            if (matches.Count == 3)
            {
                data.Value1 = Convert.ToInt32(matches[0].Value);
                data.Value2 = Convert.ToInt32(matches[1].Value);
                data.Id = Convert.ToInt64(matches[2].Value);
            }
            else
            {
                throw new Exception("Invalid maches count");
            }

            return data;
        }

        private static string GetSubmitChallande(ChallengeData data)
        {
            var postData = string.Format("payload={0}&contact={1}&id={2}", data.Calculate().ToString(),
                                         Uri.EscapeDataString("kappy@acydburne.com.pt"), data.Id.ToString());
            Console.WriteLine(postData);
            var request = (HttpWebRequest)WebRequest.Create(new Uri(Url, Answer));
            request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
            byte[] bytes = Encoding.UTF8.GetBytes(postData);
			request.ContentLength = bytes.Length;
			Stream requestStream = request.GetRequestStream();
			requestStream.Write(bytes, 0, bytes.Length);


            var response = (HttpWebResponse)request.GetResponse();
            using (var responseStream = response.GetResponseStream())
            {
                using (var streamReader = new StreamReader(responseStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }

    public class ChallengeData
    {
        public long Id { get; set; }
        public int Value1 { get; set; }
        public int Value2 { get; set; }
        public ChallengeOperation Operator { get; set; }

        public int Calculate()
        {
            if (Operator == ChallengeOperation.Add)
            {
                return Value1 + Value2;
            }
            if (Operator == ChallengeOperation.Subtract)
            {
                return Value2 - Value1;
            }
            throw new Exception("Invalid operation on calculation");
        }
    }

    public enum ChallengeOperation
    {
        Subtract,
        Add
    };
}
