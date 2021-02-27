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
        public CSSMS ()
        {
        }

        public String SendMessage(string toPhone, string msg)
        {
            var accountSid = "AC0f7677eaacbcd57b526caf812170880d";
            var authToken = "379a2526209b39eaef149c24ee5968a9";

            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
                body: msg,
                from: new Twilio.Types.PhoneNumber("+13606901260"),
                to: new Twilio.Types.PhoneNumber(toPhone)
            );

            return message.Sid;
        }
    }
}
