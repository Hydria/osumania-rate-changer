using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace osutosmconverter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void input_btn_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Select the .osu file";
            fdlg.InitialDirectory = @"c:\";
            fdlg.Filter = "Osu files (*.osu)|*.osu|Osu files (*.osu)|*.osu";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = fdlg.FileName;
            }
        }

        private void convert_btn_Click(object sender, EventArgs e)
        {
            //imports all lines of the text into a string array
            string[] lines = System.IO.File.ReadAllLines(textBox1.Text);

            //error checking
            string fullmode = lines[9];
            string mode = fullmode.Substring(6);
            if (mode != "3")
            {
                MessageBox.Show("This is not an osu!mania file. Please choose an osu!mania file.");
            }
            else
            {
                //changes the BPM value entered into one I can use
                decimal speedchange = Convert.ToDecimal(textBox3.Text);
                decimal totalspeed = 100 + speedchange;
                decimal totalspeedchange = (totalspeed) / 100;
                //audio filename setting
                lines[3] = "AudioFilename: " + textBox4.Text + ".mp3";
                // ----- LOCATE DIFFICULTY ----- //
                int j = 1;
                int difficulty = 0;
                string creator = "Creator:";
                foreach (string x in lines)
                {
                    if (x.Contains(creator))
                    {
                        difficulty = j;
                    }
                    else
                    {
                        j++;
                    }
                }
                // ----- LOCATE TIMING POINTS ----- //
                int i = 1;
                int offset = 0;
                string timingpoints = "[TimingPoints]";
                foreach (string x in lines)
                {
                    if (x.Contains(timingpoints))
                    {
                        offset = i;
                    }
                    else
                    {
                        i++;
                    }
                }
                // ----- LOCATE HIT OBJECTS ----- //
                int m = -2;
                int fulltime = 0;
                string hitobjects = "[HitObjects]";
                foreach (string x in lines)
                {
                    if (x.Contains(hitobjects))
                    {
                        fulltime = m;
                    }
                    else
                    {
                        m++;
                    }
                }

                // ----- TEXT BEFORE DIFFICULTY NAME ----- //
                string firstbigbitoftext = null;
                string stringdiff = lines[difficulty];
                for (int k = 2; k < difficulty; k++)
                {
                    firstbigbitoftext = firstbigbitoftext + lines[k] + Environment.NewLine;
                }

                // ----- TEXT BEFORE TIMING POINTS ----- //
                string secondbigbitoftext = null;
                for (int l = (difficulty + 1); l < offset; l++)
                {
                    secondbigbitoftext = secondbigbitoftext + lines[l] + Environment.NewLine;
                }

                // ---- TEXT BEFORE HIT OBJECTS ----- //
                string thirdbigbitoftext = null;
                for (int n = offset; n < fulltime; n++)
                {
                    //changes the first value (offset) to the desired rate and adds the value to the string
                    string[] linearray = lines[n].Split(new char[] { ',' });
                    string firstvalue = linearray[0];
                    decimal decfirstvalue = Convert.ToDecimal(firstvalue);
                    decfirstvalue = decfirstvalue / totalspeedchange;
                    decfirstvalue = Math.Floor(decfirstvalue);
                    thirdbigbitoftext = thirdbigbitoftext + decfirstvalue + ",";

                    //checks the second value to see if it's a BPM change or an SV change and if 
                    //it's a BPM change, works out the new BPM and adds the value to the string.
                    string secondvalue = linearray[1];
                    decimal decsecondvalue = Convert.ToDecimal(secondvalue);
                    if (decsecondvalue > 0)
                    {
                        decimal resetvalue = 60000 / decsecondvalue;
                        decimal newvalue = resetvalue * totalspeedchange;
                        decsecondvalue = 60000 / newvalue;
                    }
                    thirdbigbitoftext = thirdbigbitoftext + decsecondvalue + ",";

                    //does a for loop for the rest of the objects in the line
                    for (int o = 2; o < linearray.Length; o++)
                    {
                        thirdbigbitoftext = thirdbigbitoftext + linearray[o] + ",";
                    }
                    //removes the last comma and adds a new line
                    thirdbigbitoftext = thirdbigbitoftext.Truncate(thirdbigbitoftext.Length - 1);
                    thirdbigbitoftext = thirdbigbitoftext + Environment.NewLine;

                }

                // ----- REST OF TEXT ----- //
                string fourthbigbitoftext = null;
                for (int p = fulltime + 3; p < (lines.Length-1); p++)
                {
                    string[] linearray = lines[p].Split(new char[] { ',' });
                    fourthbigbitoftext = fourthbigbitoftext + linearray[0] + "," + linearray[1] + ",";
                    string thirdvalue = linearray[2];
                    decimal decthirdvalue = Convert.ToDecimal(thirdvalue);
                    decthirdvalue = decthirdvalue / totalspeedchange;
                    decthirdvalue = Math.Floor(decthirdvalue);
                    fourthbigbitoftext = fourthbigbitoftext + decthirdvalue + ",";
                    //need to check if it's an LN now since LNs need a different edit to normal notes.
                    string linecheck = linearray[3];
                    if (linecheck == "128")
                    {
                        fourthbigbitoftext = fourthbigbitoftext + linearray[3] + "," + linearray[4] + ",";
                        string linefind = linearray[5];
                        string[] secondlinearray = linefind.Split(new char[] { ':' });
                        string fifthvalue = secondlinearray[0];
                        decimal decfifthvalue = Convert.ToDecimal(fifthvalue);
                        decfifthvalue = decfifthvalue / totalspeedchange;
                        decfifthvalue = Math.Floor(decfifthvalue);
                        fourthbigbitoftext = fourthbigbitoftext + decfifthvalue + ":0:0:0:0:";
                    }
                    else
                    {
                        for (int q = 3; q < linearray.Length; q++)
                        {
                            fourthbigbitoftext = fourthbigbitoftext + linearray[q] + ",";
                        }
                        fourthbigbitoftext = fourthbigbitoftext.Truncate(fourthbigbitoftext.Length - 1);
                    }
                    fourthbigbitoftext = fourthbigbitoftext + Environment.NewLine;
                }

                MessageBox.Show(firstbigbitoftext + Environment.NewLine + secondbigbitoftext);
                string fileoutput = "osu file format v14 " + Environment.NewLine +
                    "//Conversion completed by Hydria. http://www.hydriaslist.ga/ " + Environment.NewLine + Environment.NewLine +
                    firstbigbitoftext + stringdiff + " " + totalspeedchange + "x" + Environment.NewLine + secondbigbitoftext +
                    thirdbigbitoftext + Environment.NewLine + Environment.NewLine + "[HitObjects]" + Environment.NewLine + fourthbigbitoftext;

                int outputfilelength = textBox1.Text.Length;
                string firstoutputfile = textBox1.Text.Truncate(outputfilelength - 5);

                string outputfile = firstoutputfile + " " + totalspeedchange + "x].osu";
                File.WriteAllText(outputfile, fileoutput);
                MessageBox.Show("Conversion Complete.");
            }
        }
    }
}
public static class StringExt
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
