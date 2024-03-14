using Assets.Scripts.Gameplay;
using System;
using System.Collections.Generic;
using UnityEngine;
using LSL;
using System.IO;

public class LSLSender
{
    public static StreamInfo info = new StreamInfo("VR_GAME", "STIM", 1, 100, LSL.channel_format_t.cf_float32, "VR");
    public static StreamOutlet outlet;

    public static StreamInfo info_handpos = new StreamInfo("Hand_POS", "MISC", 1, 100, LSL.channel_format_t.cf_float32, "VR");
    public static StreamOutlet outlet_handpos;

    private static string logFilePathPattern = @"lsl_logs\lsl_log_{0} ({1}).log";
    private static string logFilePath = "";

    public static void init()
    {
        outlet = new StreamOutlet(info);
        outlet_handpos = new StreamOutlet(info_handpos);

        int i = 1;
        logFilePath = String.Format(logFilePathPattern, getDateTimeFormatted(false), i); 
        while (File.Exists(logFilePath))
        {
            logFilePath = String.Format(logFilePathPattern, getDateTimeFormatted(false), ++i);
        }
    }

    public static async void SendLsl(string title, float[] data)
    {

        // create stream info and outlet
        
        outlet.push_sample(data);

        string report = "VR LSL sent: " + data[0].ToString() + "(" + title + ")";
        Debug.LogWarning(report);
        string[] reps = new string[] { "[" + getDateTimeFormatted(true) + "] " + report };
        File.AppendAllLines(logFilePath, reps);
    }

    public static async void SendLslForHandPos(string title, float[] data)
    {
        // create stream info and outlet
        outlet_handpos.push_sample(data);

        Debug.LogWarning("VR Hand_Pos LSL sent: " + 
            //data[0].ToString() + 
            "  (" + title + ")");
    }

    public static float[] Data = {-1};

    public static void SendLslForTask()
    {

        // create stream info and outlet
        StreamInfo info = new StreamInfo("TaskStart", "EEG", 1, 100, LSL.channel_format_t.cf_float32, "VR");
        StreamOutlet outlet = new StreamOutlet(info);
        outlet.push_sample(Data);

    }

    private static string getDateTimeFormatted(bool timeIncluded)
    {
        DateTime now = DateTime.Now;
        string dateTimeString = now.Year.ToString() + "_" + now.Month.ToString("00") + "_" + now.Day.ToString("00");
        if (timeIncluded)
        {
            dateTimeString += "_" + now.Hour.ToString("00") + "_" + now.Minute.ToString("00") + "_" + now.Second.ToString("00") + "_" + now.Millisecond.ToString("000");
        }

        return dateTimeString;
    }
}