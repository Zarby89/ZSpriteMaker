using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ZSpriteMaker
{
    internal class LogicControl
    {
        public UIElement uiElement { get; set; }
        public LogicControl ConnectedControl { get; set; } // Connected Main Control
        public LogicControl ConnectedBranchControl { get; set; } // Connected Branch Control

        public bool isRoot = false;
        public Point MainIn { get; set; }
        public Point MainOut { get; set; }
        public Point BranchOut { get; set; }

        public Point ValueIn1 { get; set; }
        public Point ValueIn2 { get; set; }
        public Point ValueOut { get; set; }

        public LineConnector LineMain;
        public LineConnector LineBranch;
        public LineConnector LineValue1;
        public LineConnector LineValue2;
        private Canvas canvas { get; set; }
        public string asm { get; set; } = "";
        public int branchPos = -1;
        public string listboxLabel { get; set; } = "";
        public string listboxBranchLabel { get; set; } = "";

        public LogicControl[] values = new LogicControl[2];

        public LogicControl(string xaml, Canvas canvas)
        {
            int asmStart = xaml.IndexOf("<!--ASM>");
            int asmEnd = xaml.IndexOf("</ASM-->");


            asm = xaml.Substring(asmStart + 8, asmEnd - (asmStart + 8));



            this.canvas = canvas;
            uiElement = (UIElement)XamlReader.Parse(xaml);




            LineMain = new LineConnector(canvas);
            LineBranch = new LineConnector(canvas);
            LineValue1 = new LineConnector(canvas);
            LineValue1.line.Stroke = Brushes.Purple;
            LineValue2 = new LineConnector(canvas);
            LineValue2.line.Stroke = Brushes.Purple;
        }

        public void MoveControl()
        {
            getEllipsePoints();
            updateLinePos();
        }

        public void getEllipsePoints()
        {
            foreach (UIElement control in Editor.FindVisualChilds<UIElement>(uiElement))
            {
                if (control is Ellipse)
                {
                    Ellipse ellipse = (Ellipse)control;
                    if (ellipse.Tag.ToString() == "mainin")
                    {
                        MainIn = control.TranslatePoint(new Point(8, 8), canvas);

                    }
                    else if (ellipse.Tag.ToString() == "mainout")
                    {
                        MainOut = control.TranslatePoint(new Point(8, 8), canvas);

                    }
                    else if (ellipse.Tag.ToString() == "branchout")
                    {
                        BranchOut = control.TranslatePoint(new Point(8, 8), canvas);
                    }
                    else if (ellipse.Tag.ToString() == "valuein1")
                    {
                        ValueIn1 = control.TranslatePoint(new Point(8, 8), canvas);
                    }
                    else if (ellipse.Tag.ToString() == "valuein2")
                    {
                        ValueIn2 = control.TranslatePoint(new Point(8, 8), canvas);
                    }
                    else if (ellipse.Tag.ToString() == "valueout")
                    {
                        ValueOut = control.TranslatePoint(new Point(8, 8), canvas);

                    }
                }
            }
        }

        public void updateLinePos()
        {
            if (ConnectedControl != null)
            {
                LineMain.updateLinePos(MainOut, ConnectedControl.MainIn);
            }
            else
            {
                LineMain.updateLinePos(MainOut, MainOut);
            }
            if (ConnectedBranchControl != null)
            {
                LineBranch.updateLinePos(BranchOut, ConnectedBranchControl.MainIn);
            }
            else
            {
                LineBranch.updateLinePos(BranchOut, BranchOut);
            }

            if (values[0] != null)
            {
                LineValue1.updateLinePos(ValueIn1, values[0].ValueOut);
            }
            else
            {
                LineValue1.updateLinePos(ValueIn1, ValueIn1);
            }

            if (values[1] != null)
            {
                LineValue2.updateLinePos(ValueIn2, values[1].ValueOut);
            }
            else
            {
                LineValue2.updateLinePos(ValueIn2, ValueIn2);
            }

        }

        public string GetASMBeforeBranch()
        {
            string s = asm;
            int branchPos = asm.IndexOf("SPRed.AddBranch");
            if (branchPos != -1)
            {
                s = asm.Substring(0, branchPos);
            }

            return s;
        }

        public string GetASMAfterBranch()
        {
            int branchPos = asm.IndexOf("SPRed.AddBranch");
            string s = asm.Substring(branchPos + 15);
            if (branchPos == -1)
            {
                return "";
            }
            return s;
        }

        public void GenerateASM()
        {
            Dictionary<string, string> variableValues = new Dictionary<string, string>();
            foreach (ComboBox tb in Editor.FindVisualChilds<ComboBox>(uiElement))
            {
                if (tb.Tag != null)
                {
                    string vn = (tb.Tag as string);
                    if (tb.SelectedIndex != -1)
                    {
                        if ((tb.SelectedItem as ComboBoxItem).Tag != null)
                        {
                            variableValues.Add(vn, ((tb.SelectedItem as ComboBoxItem).Tag as string));
                        }
                        else
                        {
                            variableValues.Add(vn, tb.SelectedIndex.ToString());
                        }
                    }
                }
            }

            foreach (TextBox tb in Editor.FindVisualChilds<TextBox>(uiElement))
            {
                if (tb.Tag != null)
                {
                    string vn = (tb.Tag as string);
                    variableValues.Add(vn, tb.Text);
                }
            }

            foreach (CheckBox tb in Editor.FindVisualChilds<CheckBox>(uiElement)) // return 0:1 unless specified tag
            {
                if (tb.Tag != null)
                {

                    string vn = (tb.Tag as string);
                    string[] mvn = vn.Split('|');
                    string[] checkboxvalue = mvn[1].Split(':');
                    string cValue = "";
                    if (tb.IsChecked == true)
                    {
                        if (checkboxvalue.Length == 1)
                        {
                            cValue = "1";
                        }
                        else
                        {
                            cValue = checkboxvalue[1];
                        }
                    }
                    else
                    {
                        if (checkboxvalue.Length == 1)
                        {
                            cValue = "0";
                        }
                        else
                        {
                            cValue = checkboxvalue[0];
                        }
                    }
                    variableValues.Add(mvn[0], cValue);
                }
            }


            //Console.WriteLine("Value of combobox = " + variableValues["TimerComboBox"]);
            //HERE Parse the ASM section to find the symbol+Key replace it with Value
            foreach (var v in variableValues)
            {
                string[] str;
                if (v.Value.Contains('|')) // need to split
                {
                    str = v.Value.Split('|');
                    for (int i = 0; i < str.Length; i++)
                    {
                        asm = asm.Replace("SPRed." + v.Key + "[" + i + "]", str[i]);
                        listboxLabel = listboxLabel.Replace("SPRed." + v.Key + "[" + i + "]", str[i]);
                    }
                }
                else
                {
                    asm = asm.Replace("SPRed." + v.Key, v.Value);
                    listboxLabel = listboxLabel.Replace("SPRed." + v.Key, v.Value);
                }

                //Console.WriteLine(v.Key);
            }

        }
    }
}
