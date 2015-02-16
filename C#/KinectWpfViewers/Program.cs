﻿//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// This module provides sample code used to demonstrate the use
// of the KinectAudioSource for speech recognition

// IMPORTANT: This sample requires the Speech Platform SDK (v11) to be installed on the developer workstation

namespace Speech
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.Kinect;
    using Microsoft.Speech.AudioFormat;
    using Microsoft.Speech.Recognition;

    public class Program
    {
        static bool flag = false;
        public Program()
        {
            // Obtain a KinectSensor if any are available
            KinectSensor sensor = (from sensorToCheck in KinectSensor.KinectSensors where sensorToCheck.Status == KinectStatus.Connected select sensorToCheck).FirstOrDefault();
            if (sensor == null)
            {
                return;
            }

            sensor.Start();

            // Obtain the KinectAudioSource to do audio capture
            KinectAudioSource source = sensor.AudioSource;
            source.EchoCancellationMode = EchoCancellationMode.None; // No AEC for this sample
            source.AutomaticGainControlEnabled = false; // Important to turn this off for speech recognition

            RecognizerInfo ri = GetKinectRecognizer();

            if (ri == null)
            {
                return;
            }

            int wait = 4;

            using (var sre = new SpeechRecognitionEngine(ri.Id))
            {
                var colors = new Choices();
                colors.Add("hit");
                //colors.Add("green");
                //colors.Add("blue");

                var gb = new GrammarBuilder { Culture = ri.Culture };

                // Specify the culture to match the recognizer in case we are running in a different culture.                                 
                gb.Append(colors);

                // Create the actual Grammar instance, and then load it into the speech recognizer.
                var g = new Grammar(gb);

                sre.LoadGrammar(g);
                sre.SpeechRecognized += SreSpeechRecognized;
                sre.SpeechHypothesized += SreSpeechHypothesized;
                sre.SpeechRecognitionRejected += SreSpeechRecognitionRejected;

                using (Stream s = source.Start())
                {
                    sre.SetInputToAudioStream(
                        s, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                    sre.RecognizeAsync(RecognizeMode.Multiple);
                    sre.RecognizeAsyncStop();
                }
            }

            sensor.Stop();
        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }

        private static void SreSpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            if (e.Result != null)
            {
                DumpRecordedAudio(e.Result.Audio);
            }
        }

        private static void SreSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
        }

        private static void SreSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence >= 0.7)
            {
                if (e.Result.Text == "hit")
                {
                    flag = true;
                }
                //Console.WriteLine("\nSpeech Recognized: \t{0}\tConfidence:\t{1}", e.Result.Text, e.Result.Confidence);
            }
            else
            {
                //Console.WriteLine("\nSpeech Recognized but confidence was too low: \t{0}", e.Result.Confidence);
                DumpRecordedAudio(e.Result.Audio);
            }
        }

        public bool getHit()
        {
            bool tmp = flag;
            flag = false;
            return tmp;
        }

        private static void DumpRecordedAudio(RecognizedAudio audio)
        {
            if (audio == null)
            {
                return;
            }

            int fileId = 0;
            string filename;
            while (File.Exists((filename = "RetainedAudio_" + fileId + ".wav")))
            {
                fileId++;
            }

            using (var file = new FileStream(filename, System.IO.FileMode.CreateNew))
            {
                audio.WriteToWaveStream(file);
            }
        }
    }
}
