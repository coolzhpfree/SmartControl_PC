﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZControl.FormDeviceClass
{
    public partial class FormZDC1 : FormItem
    {
        public Boolean[] plugSwitch = new Boolean[4] { false, false, false, false }; //开关状态

        PictureBox[] picZDC1SwitchPic = new PictureBox[4];
        Label[] labZDC1SwitchName = new Label[4];

        private void Send(String message)
        {
            Send("device/zdc1/" + GetMac() + "/set", message);
        }
        public FormZDC1(String name, String mac) : base(DEVICETYPE.TYPE_DC1, name, mac)
        {
            InitializeComponent();


            picZDC1SwitchPic[0] = picZDC1Switch0;
            picZDC1SwitchPic[1] = picZDC1Switch1;
            picZDC1SwitchPic[2] = picZDC1Switch2;
            picZDC1SwitchPic[3] = picZDC1Switch3;

            labZDC1SwitchName[0] = labZDC1Switch0Name;
            labZDC1SwitchName[1] = labZDC1Switch1Name;
            labZDC1SwitchName[2] = labZDC1Switch2Name;
            labZDC1SwitchName[3] = labZDC1Switch3Name;
            for (int i = 0; i < picZDC1SwitchPic.Count(); i++)
            {
                picZDC1SwitchPic[i].Click += PicZDC1Switch_Click;
                picZDC1SwitchPic[i].Tag = i;

                picZDC1SwitchPic[i].MouseDown += PicSwitch_MouseDown;
                picZDC1SwitchPic[i].MouseUp += PicSwitch_MouseUp;
                picZDC1SwitchPic[i].MouseLeave += PicSwitch_MouseLeave;
            }


        }
        #region 重写函数
        public override String[] GetRecvMqttTopic()
        {
            String[] topic = new String[3];
            topic[0] = "device/zdc1/" + GetMac() + "/state";
            topic[1] = "device/zdc1/" + GetMac() + "/sensor";
            topic[2] = "device/zdc1/" + GetMac() + "/availability";
            return topic;
        }
        public override void Received(String topic, String message)
        {
            JObject jsonObject = JObject.Parse(message);
            if (!GetMac().Equals(jsonObject["mac"].ToString())) return;


            if (jsonObject.Property("power") != null)
            {
                labelZDC1Power.Text = jsonObject["power"].ToString() + "W";
            }

            if (jsonObject.Property("voltage") != null)
            {
                labelZDC1Voltage.Text = jsonObject["voltage"].ToString() + "V";
            }

            if (jsonObject.Property("current") != null)
            {
                labelZDC1Current.Text = jsonObject["current"].ToString() + "A";
            }
            if (jsonObject.Property("version") != null)
            {
                labelZDC1Version.Text = "固件版本: " + jsonObject["version"].ToString();
            }
            #region 解析plug
            int plugAllFlag = 0x0;
            for (int plug_id = 0; plug_id < 4; plug_id++)
            {
                if (jsonObject.Property("plug_" + plug_id) == null) continue;

                JObject jsonPlug = (JObject)jsonObject["plug_" + plug_id];
                if (jsonPlug.Property("on") != null)
                {
                    int on = (int)jsonPlug["on"];
                    plugSwitch[plug_id] = (on != 0);
                    picZDC1SwitchPic[plug_id].Image = plugSwitch[plug_id] ? Properties.Resources.device_open : Properties.Resources.device_close;
                    if (plugSwitch[plug_id]) plugAllFlag |= 0x81;
                    else plugAllFlag |= 0x80;
                }
                if (jsonPlug.Property("setting") == null) continue;
                JObject jsonPlugSetting = (JObject)jsonPlug["setting"];
                if (jsonPlugSetting.Property("name") != null)
                {
                    labZDC1SwitchName[plug_id].Text = jsonPlugSetting["name"].ToString();
                    labZDC1SwitchName[plug_id].Left = picZDC1SwitchPic[plug_id].Left + picZDC1SwitchPic[plug_id].Width / 2 - labZDC1SwitchName[plug_id].Width / 2;
                }
            }

            #endregion


        }

        public override void RefreshStatus()
        {
            Send("{\"mac\": \"" + GetMac() + "\","
                            + "\"version\":null,"
                            + "\"plug_0\" : {\"on\" : null,\"setting\":{\"name\":null}},"
                            + "\"plug_1\" : {\"on\" : null,\"setting\":{\"name\":null}},"
                            + "\"plug_2\" : {\"on\" : null,\"setting\":{\"name\":null}},"
                            + "\"plug_3\" : {\"on\" : null,\"setting\":{\"name\":null}}}");
        }

        #endregion
        #region 开关图片按下效果
        private void PicSwitch_MouseDown(object sender, MouseEventArgs e)
        {
            ((PictureBox)sender).BorderStyle = BorderStyle.Fixed3D;
        }
        private void PicSwitch_MouseUp(object sender, MouseEventArgs e)
        {
            ((PictureBox)sender).BorderStyle = BorderStyle.None;
        }
        private void PicSwitch_MouseLeave(object sender, EventArgs e)
        {
            ((PictureBox)sender).BorderStyle = BorderStyle.None;
        }
        #endregion

        private void PicZDC1Switch_Click(object sender, EventArgs e)
        {
            PictureBox zDC1SwitchPic = (PictureBox)sender;
            int index = (int)zDC1SwitchPic.Tag;

            if (index > 0)
            {
                Send("{\"mac\":\"" + GetMac() + "\",\"plug_" + index + "\":{\"on\":" + (plugSwitch[index] ? "0" : "1") + "}}");
            }
            else
            {
                if (plugSwitch[index])
                    Send("{\"mac\":\"" + GetMac() + "\",\"plug_0\":{\"on\":0},\"plug_1\":{\"on\":0},\"plug_2\":{\"on\":0},\"plug_3\":{\"on\":0}}");
                else
                    Send("{\"mac\":\"" + GetMac() + "\",\"plug_0\":{\"on\":1},\"plug_1\":{\"on\":1},\"plug_2\":{\"on\":1},\"plug_3\":{\"on\":1}}");
            }

            plugSwitch[index] = !plugSwitch[index];
            picZDC1SwitchPic[index].Image = plugSwitch[index] ? Properties.Resources.device_open : Properties.Resources.device_close;

        }

        private void linkRefresh_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            RefreshStatus();
        }
    }
}
