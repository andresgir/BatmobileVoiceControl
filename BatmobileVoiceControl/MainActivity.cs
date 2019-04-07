using Android.App;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using Lego.Ev3.Core;
using Lego.Ev3.Android;
using System;
using Newtonsoft.Json;
using Amazon.Runtime;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Linq;
using System.Threading;

namespace BatmobileVoiceControl
{
	[Activity(Label = "Batmobile Voice Control", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		int count = 1;

		uint _time = 300;
		int _distanceMeasured = Int32.MaxValue;

        int speed = 10;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button>(Resource.Id.myButton);

			button.Click += delegate
			{
				button.Text = "Alexa is commanding now...";

				Configure();
				Task t = Execute();
				//t.Wait();
				//System.Console.ReadKey();

			};



		}

		#region CONSTANTS

		private const int TIMEOUT_IN_MS = 125;
		private const string ACCESS_KEY_ENV_NAME = "";
		private const string SECRET_KEY_ENV_NAME = "";
		private const string EV3_PORT_KEY = "Ev3Port";
		private const string AWS_SQS_ADDRESS_KEY = "AwsSqsAddress";

		#endregion

		#region PRIVATE VARIABLES

		private static string _ev3Port = "EV3";
		private static string _awsSqsAddress;
		private static AmazonSQSClient _sqsClient;
		private static Brick _brick;

		#endregion

		//public static void Main(string[] args)
		//{
		//    Configure();
		//    Task t = Execute();
		//    t.Wait();
		//    System.Console.ReadKey();
		//}

		private async Task Execute()
		{
			_brick = new Brick(new BluetoothCommunication("EV3"));
			_brick.BrickChanged += _brick_BrickChanged;

			System.Console.WriteLine("Connecting...");
			await _brick.ConnectAsync();

			System.Console.WriteLine("Connected... Waiting for Commands...");
			await _brick.DirectCommand.PlayToneAsync(0x50, 5000, 500);

			//await _brick.SystemCommand.CopyFileAsync("test.rsf", "../prjs/Program/test.rsf");

			//await _brick.DirectCommand.PlaySoundAsync(100, "../prjs/Program/test");


			while (true)
			{

				Ev3Command command = PollForQueueMessage();


				if (command != null)
				{
					await ProcessCommandAsync(command);
				}
			}
		}

		private async Task ProcessCommandAsync(Ev3Command command)
		{
			switch (command.Action)
			{

				case "start":
                    await StartEngines(command);
					break;
				case "engines":
					await StartEngines(command);
					break;
                case "forward":
					await MoveForward(command);
					break;
				case "straight":
					await MoveForward(command);
					break;
				case "ahead":
					await MoveForward(command);
					break;
				case "backward":
					await MoveBackward(command);
					break;
				case "reverse":
					await MoveBackward(command);
					break;
				case "back":
					await MoveBackward(command);
					break;
				case "center":
					await ReturnToCenter(command);
					break;
				case "left":
					await MoveLeft(command);
					break;
				case "right":
					await MoveRight(command);
					break;
				case "stop":
                    await StopMoving();
					break;
				case "don't move":
					await StopMoving();
					break;
				case "do not move":
					await StopMoving();
					break;
				case "break":
					await StopMoving();
					break;
				case "fast":
					await GoFaster(command);
					break;
				case "faster":
					await GoFaster(command);
					break;
				case "accelerate":
					await GoFaster(command);
					break;
				case "increase":
					await GoFaster(command);
					break;
				case "slow":
                    await GoSlower(command);
					break;
				case "slower":
					await GoSlower(command);
					break;
				case "down":
					await GoSlower(command);
					break;
				case "decrease":
					await GoSlower(command);
					break;
				case "fire":
					await FireGun(command);
					break;
				case "shoot":
					await FireGun(command);
					break;
				case "blast":
					await FireGun(command);
					break;
				// Add more commands here as needed.
				default:
					break;
			}

			System.Console.WriteLine("Command executed.");
		}


		private async Task StartEngines(Ev3Command command)
		{
			Console.WriteLine("Starting Engines...");

			uint distance = 30;
			if (!string.IsNullOrWhiteSpace(command.Value))
			{
				uint.TryParse(command.Value, out distance);
			}

			distance *= 10;
			_brick.BatchCommand.Initialize(CommandType.DirectNoReply);
			_brick.BatchCommand.SetMotorPolarity(OutputPort.A, Polarity.Forward);
			_brick.BatchCommand.SetMotorPolarity(OutputPort.D, Polarity.Forward);
			_brick.BatchCommand.StepMotorAtPower(OutputPort.A, 10, distance, false);
			_brick.BatchCommand.StepMotorAtPower(OutputPort.D, 10, distance, false);
			await _brick.BatchCommand.SendCommandAsync();
		}



		private async Task MoveForward(Ev3Command command)
		{
			Console.WriteLine("Moving Forward...");

			uint distance = 30;
			if (!string.IsNullOrWhiteSpace(command.Value))
			{
				uint.TryParse(command.Value, out distance);
			}

			//if(_distanceMeasured <= 30)
			//{
			//    distance = 0;
			//}

			if (speed == 10)
			{
				distance *= 1000;
				_brick.BatchCommand.Initialize(CommandType.DirectNoReply);
				_brick.BatchCommand.SetMotorPolarity(OutputPort.A, Polarity.Forward);
				_brick.BatchCommand.SetMotorPolarity(OutputPort.D, Polarity.Forward);
				_brick.BatchCommand.StepMotorAtPower(OutputPort.A, 10, distance, false);
				_brick.BatchCommand.StepMotorAtPower(OutputPort.D, 10, distance, false);
				await _brick.BatchCommand.SendCommandAsync();
				speed += 5;
			}


			distance *= 100;
			_brick.BatchCommand.Initialize(CommandType.DirectNoReply);
			_brick.BatchCommand.SetMotorPolarity(OutputPort.A, Polarity.Forward);
			_brick.BatchCommand.SetMotorPolarity(OutputPort.D, Polarity.Forward);
			_brick.BatchCommand.StepMotorAtPower(OutputPort.A, 100, distance, false);
			_brick.BatchCommand.StepMotorAtPower(OutputPort.D, 100, distance, false);
			await _brick.BatchCommand.SendCommandAsync();
		}


		
		private async Task MoveBackward(Ev3Command command)
		{
			Console.WriteLine("Moving Backward...");

			uint distance = 30;
			if (!string.IsNullOrWhiteSpace(command.Value))
			{
				uint.TryParse(command.Value, out distance);
			}

			distance *= 100;
			_brick.BatchCommand.Initialize(CommandType.DirectNoReply);
			_brick.BatchCommand.SetMotorPolarity(OutputPort.A, Polarity.Backward);
			_brick.BatchCommand.SetMotorPolarity(OutputPort.D, Polarity.Backward);
			//_brick.BatchCommand.StepMotorAtPower(OutputPort.A, 100, distance, false);
			//_brick.BatchCommand.StepMotorAtPower(OutputPort.D, 100, distance, false);
			await _brick.BatchCommand.SendCommandAsync();
		}

		private async Task StopMoving()
		{
			Console.WriteLine("Stoping car...");

			//uint distance = 30;
			//if (!string.IsNullOrWhiteSpace(command.Value))
			//{
			//	uint.TryParse(command.Value, out distance);
			//}

			//distance *= 100;
			_brick.BatchCommand.Initialize(CommandType.DirectNoReply);
            _brick.BatchCommand.StopMotor(OutputPort.A, false);
            _brick.BatchCommand.StopMotor(OutputPort.D, false);
			await _brick.BatchCommand.SendCommandAsync();
		}

		private async Task GoFaster(Ev3Command command)
		{
			Console.WriteLine("Moving faster...");

			//int speed = 30;
            uint distance = 30;


			if (!string.IsNullOrWhiteSpace(command.Value))
			{
                uint.TryParse(command.Value, out distance);
			}

            if (speed == 10)
			{
                distance *= 1000;
				_brick.BatchCommand.Initialize(CommandType.DirectNoReply);
				_brick.BatchCommand.SetMotorPolarity(OutputPort.A, Polarity.Forward);
				_brick.BatchCommand.SetMotorPolarity(OutputPort.D, Polarity.Forward);
				_brick.BatchCommand.StepMotorAtPower(OutputPort.A, 3, distance, false);
				_brick.BatchCommand.StepMotorAtPower(OutputPort.D, 3, distance, false);
				await _brick.BatchCommand.SendCommandAsync();
                speed += 5;
            }else {


				speed += 5;
				distance *= 1000;
				_brick.BatchCommand.Initialize(CommandType.DirectNoReply);
				//         _brick.BatchCommand.SetMotorPolarity(OutputPort.A, Polarity.Forward);
				//_brick.BatchCommand.SetMotorPolarity(OutputPort.D, Polarity.Forward);
				_brick.BatchCommand.StepMotorAtSpeed(OutputPort.A, speed, distance, false);
				_brick.BatchCommand.StepMotorAtSpeed(OutputPort.D, speed, distance, false);
				await _brick.BatchCommand.SendCommandAsync();


            }


		}

		private async Task GoSlower(Ev3Command command)
		{
			Console.WriteLine("Moving slower...");

			uint distance = 30;

			if (!string.IsNullOrWhiteSpace(command.Value))
			{
				uint.TryParse(command.Value, out distance);
			}

			if (speed == 10)
			{
				distance *= 10;
				_brick.BatchCommand.Initialize(CommandType.DirectNoReply);
				_brick.BatchCommand.SetMotorPolarity(OutputPort.A, Polarity.Forward);
				_brick.BatchCommand.SetMotorPolarity(OutputPort.D, Polarity.Forward);
				_brick.BatchCommand.StepMotorAtPower(OutputPort.A, 10, distance, false);
				_brick.BatchCommand.StepMotorAtPower(OutputPort.D, 10, distance, false);
				await _brick.BatchCommand.SendCommandAsync();
			}

			speed -= 5;
			distance *= 1000;
			_brick.BatchCommand.Initialize(CommandType.DirectNoReply);
			//_brick.BatchCommand.SetMotorPolarity(OutputPort.A, Polarity.Forward);
			//_brick.BatchCommand.SetMotorPolarity(OutputPort.D, Polarity.Backward);
			_brick.BatchCommand.StepMotorAtSpeed(OutputPort.A, speed, distance, false);
			_brick.BatchCommand.StepMotorAtSpeed(OutputPort.D, speed, distance, false);
			await _brick.BatchCommand.SendCommandAsync();
		}

		private async Task MoveLeft(Ev3Command command)
		{
			Console.WriteLine("Moving Left...");

			_brick.BatchCommand.Initialize(CommandType.DirectNoReply);
			_brick.BatchCommand.SetMotorPolarity(OutputPort.C, Polarity.Forward);
			//_brick.BatchCommand.SetMotorPolarity(OutputPort.C, Polarity.Forward);
            //_brick.BatchCommand.StepMotorAtSpeed(OutputPort.C, 100, 50, false);
            _brick.BatchCommand.StepMotorAtSpeed(OutputPort.C, 70, 33, false);


			//await _brick.SystemCommand.CopyFileAsync("test.rsf", "../prjs/myapp/test.rsf");

			//_brick.BatchCommand.PlaySound(100, "../prjs/myapp/test");

			//_brick.BatchCommand.StepMotorAtPower(OutputPort.C, 100, 180, false);
			await _brick.BatchCommand.SendCommandAsync();
		}

		private async Task MoveRight(Ev3Command command)
		{
			Console.WriteLine("Moving Right...");

			_brick.BatchCommand.Initialize(CommandType.DirectNoReply);
			_brick.BatchCommand.SetMotorPolarity(OutputPort.C, Polarity.Backward);
			//_brick.BatchCommand.SetMotorPolarity(OutputPort.C, Polarity.Backward);
			//_brick.BatchCommand.StepMotorAtSpeed(OutputPort.C, 70, 50, false);
            _brick.BatchCommand.StepMotorAtSpeed(OutputPort.C, 70, 33, false);

			//_brick.BatchCommand.StepMotorAtPower(OutputPort.C, 100, 180, false);
			await _brick.BatchCommand.SendCommandAsync();
		}

		private async Task ReturnToCenter(Ev3Command command)
		{
			Console.WriteLine("Return to center...");

			_brick.BatchCommand.Initialize(CommandType.DirectNoReply);
			_brick.BatchCommand.SetMotorPolarity(OutputPort.C, Polarity.Backward);
			//_brick.BatchCommand.SetMotorPolarity(OutputPort.C, Polarity.Backward);
			//_brick.BatchCommand.StepMotorAtSpeed(OutputPort.C, 70, 50, false);
			_brick.BatchCommand.StepMotorAtPower(OutputPort.C, 100, 53, false);

			//_brick.BatchCommand.StepMotoputPort.C, 100, 180, false);
			await _brick.BatchCommand.SendCommandAsync();
		}

		private async Task FireGun(Ev3Command command)
		{
			Console.WriteLine("Firing Gun...");

			uint distance = 30;
			if (!string.IsNullOrWhiteSpace(command.Value))
			{
				uint.TryParse(command.Value, out distance);
			}

			distance *= 500;

			//do
			//{

                System.Console.WriteLine(_distanceMeasured);
				_brick.BatchCommand.Initialize(CommandType.DirectNoReply);
				_brick.BatchCommand.SetMotorPolarity(OutputPort.B, Polarity.Forward);
				//_brick.BatchCommand.SetMotorPolarity(OutputPort.C, Polarity.Backward);
				_brick.BatchCommand.StepMotorAtSpeed(OutputPort.B, 50, distance, false);
				//_brick.BatchCommand.StepMotorAtPower(OutputPort.C, 100, 180, false);
				await _brick.BatchCommand.SendCommandAsync();


			//}

			//while (_distanceMeasured >= 30);


		}

		private void _brick_BrickChanged(object sender, BrickChangedEventArgs e)
		{


			_distanceMeasured = (int)e.Ports[InputPort.Four].SIValue;
			//txtDistance.Text = _distanceMeasured.ToString();

			_brick.BatchCommand.Initialize(CommandType.DirectNoReply);
			_brick.BatchCommand.StopMotor(OutputPort.A, false);
			_brick.BatchCommand.StopMotor(OutputPort.D, false);
			 _brick.BatchCommand.SendCommandAsync();

			if (_distanceMeasured <= 30)
			{
				_brick.DirectCommand.StopMotorAsync(OutputPort.All, true);
                //StopMoving();


				System.Console.WriteLine("StopMotor");
			}

		}

		private static Ev3Command PollForQueueMessage()
		{

			ReceiveMessageRequest request = new ReceiveMessageRequest();
			request.QueueUrl = "https://sqs.us-east-1.amazonaws.com/617887864731/ev3batmobilecommand";

			while (true)
			{
				DateTime timeout = DateTime.Now.AddMilliseconds(TIMEOUT_IN_MS);
				var responseTask = _sqsClient.ReceiveMessageAsync(request);

				// TODO: Replace with proper async completion handling.
				while (!responseTask.IsCompleted && DateTime.Now < timeout) { }

				ReceiveMessageResponse response = responseTask.Result;

				if (response.Messages != null && response.Messages.Count > 0)
				{
					Amazon.SQS.Model.Message nextMessage = response.Messages.First();
					DeleteMessageRequest deleteRequest = new DeleteMessageRequest();
					deleteRequest.QueueUrl = _awsSqsAddress;
					deleteRequest.ReceiptHandle = nextMessage.ReceiptHandle;


					timeout = DateTime.Now.AddMilliseconds(TIMEOUT_IN_MS);
					var deleteTask = _sqsClient.DeleteMessageAsync(deleteRequest);
					while (!deleteTask.IsCompleted && DateTime.Now < timeout) { }

					Console.WriteLine("Message: ");
					Console.WriteLine("== " + nextMessage.Body);

					var command = GetEv3CommandFromJson(nextMessage.Body);
					return command;
				}
			}

		}

		private static Ev3Command GetEv3CommandFromJson(string text)
		{

			var message = JsonConvert.DeserializeObject<SQSMessage>(text);

			var command = JsonConvert.DeserializeObject<Ev3Command>(message.Message);


			//Console.WriteLine("Found action/value pair of {0} / {1}.", action, val);

			//return new Ev3Command { Action = action, Value = val };
			return command;
		}

		private static void Configure()
		{
			//var appSettings = ConfigurationManager.AppSettings;
			//_ev3Port = appSettings[EV3_PORT_KEY] ?? "/dev/tty.EV3-SerialPort";
			_awsSqsAddress = "https://sqs.us-east-1.amazonaws.com/617887864731/ev3batmobilecommand";

			string accessKey = "AKIAIXAMLQV6L4AWHKVQ";
			string secretKey = "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";

			RegionEndpoint endpoint = RegionEndpoint.USEast1;
			AWSCredentials credentials = new BasicAWSCredentials(accessKey, secretKey);

			_sqsClient = new AmazonSQSClient(credentials, endpoint);
		}
	}
}