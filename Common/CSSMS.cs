using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Twilio.TwiML;
using Twilio.AspNet.Mvc;

namespace CStat.Common
{
    public class CSSMS
    {
        private readonly IConfiguration _configuration;
        public CSSMS (IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public String SendMessage(string toPhone, string msg)
        {
            var accountSid = _configuration["Twilio:AccountSID"];
            var authToken = _configuration["Twilio:AuthToken"];

            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
                body: msg,
                from: new Twilio.Types.PhoneNumber(_configuration["Twilio:PhoneNum"]),
                to: new Twilio.Types.PhoneNumber(toPhone)
            );

            return message.Sid;
        }
    }
}
