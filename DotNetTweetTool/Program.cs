// See https://aka.ms/new-console-template for more information


using TweetTesting;

var consumerKey = "";
var consumerSecret = "";
var accessToken = "";
var accessTokenSecret = "";

var message = "hello from KinsonDigital";

var service = new TweetService();

await service.SendTweetAsync(message, consumerKey, consumerSecret, accessToken, accessTokenSecret);

Console.WriteLine("Tweet Sent!!");
Console.ReadLine();
