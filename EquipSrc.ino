// Visual Micro is in vMicro>General>Tutorial Mode
// 
/*
    Name:       EquipSrc.ino
    Created:	12/28/2020 10:22:44 PM
    Author:     DESKTOP-SB3B88M\ron
*/

// Define User Types below here or use a .h file
//
#include <Bridge.h>
#include <HttpClient.h>
#include <BridgeClient.h>
#include <BridgeServer.h>

// Define Function Prototypes that use User Types below here or use a .h file
//


// Define Functions below here or use other .ino or cpp files
//

// The setup() function runs once each time the micro-controller starts
void setup()
{
    pinMode(13, OUTPUT);
    digitalWrite(13, LOW);

    Bridge.begin();

    Serial.begin(9600);


    Serial.print("Serial Start");

    while (!Serial);
}

// Add the main program code into the continuous loop() function
void loop()
{
    HttpClient gClient;
    //HttpClient pClient;
    BridgeClient pClient;

    Serial.println("Start Get *********");

    //gClient.get("https://ccaserve.org/api/equip");
    //gClient.get("https://192.168.1.146/api/equip");    
    //gClient.get("https://data.tankutility.com/api/devices/003a0028363537330b473931?token=AOvuKvQOF1bJAvTFsRteACow8T6jcFbOn0KHsqaYfb2SYGDJAPGkUcz5toFJqQT6B-qHXe3hr2UaV9uzxQW1U2Ade7CaAtoQH_XkO4AJ0WU2VY3d62TFTmEvUVoXle1sINNrO0lEW3mX9O-WEbK0q6oiKvExGgPV8ndp-4hwncJDByH6105vrw-jiGMtdQ5Q9CGeIh7SxwuO0aLTZuzur_FMvv4A9iC7OQAQ9HyVGvLSx0vrjju-bfI");
    //Serial.println("End Get");
    //String getResp = "";
    //while (gClient.available()) {
    //    char c = gClient.read();
    //    getResp += c;
    //}
    //Serial.println();
    //Serial.println("End Get *******");
    //Serial.flush();

    //Serial.println("Start Put *******");
    //pClient.get("https://ccaserve.org/api/equip/5?frzTemp=21&refTemp=42&wtrPres=37&");
    //while (pClient.available()) {
    //    char c = pClient.read();
    //    Serial.print(c);
    //}
    //Serial.println();
    //Serial.println("End Put *******");
    //Serial.flush();

    //String PostData = "temperature=62";
    //pClient.noCheckSSL();
    //unsigned int res = 0;
    //pClient.postAsynchronously("https://www.ccaserve.org/api/equip", "Content-Type: application/json\r\nUser-Agent: PostmanRuntime/7.26.8\r\nAccept: */*\r\nHost: www.ccaserve.org\r\nAccept - Encoding: gzip, deflate, br\r\nConnection: Close\r\nContent - Length: 13\r\n\r\n\"Name\":\"John\"");

    //    const String gPostData = "POST /api/equip HTTP/1.1\r\nHost: 52.117.175.213\r\nUser-Agent: Arduino/1.0\r\nConnection: close\r\nContent-Length: 5\r\n\r\nABCDE\r\n";
    //    unsigned int res = gClient.post("https://www.ccaserve.org/api/equip", gPostData.c_str());

    /*****
        Serial.print("Post Res=");
        Serial.println(res);
        Serial.flush();
    *****/

    //IPAddress server(52,117,175,213);
    //IPAddress server(192,168,1,146);

    String PostData = "\"freezerTemp\":21, \"frigTemp\": 102, \"waterPres\": 48";

    if (pClient.connectSSL("ccaserve.org", 443))
    {
        pClient.println("POST /api/equip HTTP/1.1");
        pClient.println("Host: www.ccaserve.org");
        pClient.println("Connection: Close");
        pClient.println("Content-Type: application/json");
        pClient.print("Content-Length: ");
        pClient.println(PostData.length());
        pClient.println();
        pClient.println(PostData);
        Serial.println("Sent " + PostData);

        // get another packet if available!!!
        String putResp = "";
        while (pClient.connected())
        {
            while (pClient.available())
            {
                char c = pClient.read();
                putResp += c;
            }
            if (putResp != "")
            {
                Serial.print("Resp=" + putResp);
                Serial.flush();
            }
        }
    }

    delay(5000);
}
