using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace Sudoku
{
    /// <summary>
    /// Interaction logic for MainGame.xaml
    /// </summary>
    public partial class MainGame : Page
    {
        static private Random rand = new Random();
        private Section section0 = new Section(0, 0, 2, 0, 2);
        private Section section1 = new Section(1, 3, 5, 0, 2);
        private Section section2 = new Section(2, 6, 8, 0, 2);
        private Section section3 = new Section(3, 0, 2, 3, 5);
        private Section section4 = new Section(4, 3, 5, 3, 5);
        private Section section5 = new Section(5, 6, 8, 3, 5);
        private Section section6 = new Section(6, 0, 2, 6, 8);
        private Section section7 = new Section(7, 3, 5, 6, 8);
        private Section section8 = new Section(8, 6, 8, 6, 8);
        List<Section> sectionList = new List<Section>();
        private List<TextBox> validTextBoxList = new List<TextBox>();
        private int numOfEditableTextBoxes;

        public MainGame(String newOrLoad)
        {
            //create structure to hold section cells
            sectionList.Add(section0);
            sectionList.Add(section1);
            sectionList.Add(section2);
            sectionList.Add(section3);
            sectionList.Add(section4);
            sectionList.Add(section5);
            sectionList.Add(section6);
            sectionList.Add(section7);
            sectionList.Add(section8);

            InitializeComponent();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    TextBox gridTextBox = new TextBox();
                    gridTextBox.TextAlignment = TextAlignment.Center;
                   // gridTextBox.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetRow(gridTextBox, i);
                    Grid.SetColumn(gridTextBox, j);
                    gridTextBox.MaxLength = 1;
                    playableGrid.Children.Add(gridTextBox);
                }
            }

            if (newOrLoad.Equals("New"))
            {
                setUpGame();
            }
            else
            {
                loadGame();
            }
             
        }


        private Section determineSection(int row, int col)
        {
            foreach (Section section in sectionList)
            {
                if (row >= section.lowerBoundRowIndex && row <= section.upperBoundRowIndex && col >= section.lowerBoundColIndex && col <= section.upperBoundColIndex)
                {
                    section.isCurrent = true;
                    return section;
                }
            }

            return null;

        }

        private void home_Click(object sender, RoutedEventArgs e)
        {
            Page homePage = new HomePage();
            Window.GetWindow(this).Content = homePage;
        }

        private void save_Click(object sender, RoutedEventArgs e) //TODO: Save into file within application, not a user defined file
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "SudokuSave"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;

                List<TextBox> allTextBoxesList = playableGrid.Children.Cast<UIElement>().OfType<TextBox>().ToList();
                String fileString = "";

                foreach(TextBox textBox in allTextBoxesList)
                {
                    fileString += textBox.Text+","+textBox.Background + ",";
                }
                File.WriteAllText(filename, fileString);
            }
        }

        private void numberEntered(object sender, RoutedEventArgs e)
        {
            TextBox changedTextBox = sender as TextBox;
            if (changedTextBox.Text.Length == 0)
            {
                changedTextBox.Background = Brushes.White;
                if (validTextBoxList.Contains(changedTextBox))
                {
                    validTextBoxList.Remove(changedTextBox);
                }
                return;
            }
            int value;
            Boolean valueIsParsable = Int32.TryParse(changedTextBox.Text, out value);
            if (!valueIsParsable || !numberEnteredIsValid(value))
            { //validate input
                MessageBox.Show("Value entered is invalid! Please enter only a digit between 1 and 9");
                changedTextBox.Clear();
            }
            else // if value is valid, continue
            {
                //check if the move is valid
                if (isMoveValid(changedTextBox))
                {
                    changedTextBox.Background = Brushes.Green;
                    if (!validTextBoxList.Contains(changedTextBox)){
                        validTextBoxList.Add(changedTextBox);
                    }
                    if(validTextBoxList.Count == numOfEditableTextBoxes)
                    {
                        runWinCondition();
                    }
                }
                else
                {
                    changedTextBox.Background = Brushes.Red;
                    if (validTextBoxList.Contains(changedTextBox))
                    {
                        validTextBoxList.Remove(changedTextBox);
                    }
                }

            }
        }

        private Boolean numberEnteredIsValid(int num)
        {
            Boolean bContainsDigit = Regex.IsMatch(num.ToString(), "^[1-9]");
            Boolean bContainsOtherCharacters = Regex.IsMatch(num.ToString(), "^[1-9].");

            Boolean isValid = bContainsDigit && !bContainsOtherCharacters;

            return isValid;
        }

        private Boolean isMoveValid(TextBox changedTextBox)
        {
            Boolean moveIsValid = false;
            String value = changedTextBox.Text;

            //get location of changed text in grid
            int colIndex = Grid.GetColumn(changedTextBox);
            int rowIndex = Grid.GetRow(changedTextBox);

            Boolean rowIsValid = isRowValid(rowIndex, value);
            Boolean colIsValid = isColValid(colIndex, value);
            Boolean sectionIsValid = isSectionValid(rowIndex, colIndex, value);

            if (rowIsValid && colIsValid && sectionIsValid)
            {
                moveIsValid = true;
            }

            return moveIsValid;
        }


        private Boolean isRowValid(int rowIndex, String value)
        {
            Boolean isValid = true;
            int matches = playableGrid.Children.Cast<UIElement>().OfType<TextBox>().Count<TextBox>(e => Grid.GetRow(e) == rowIndex && e.Text.Equals(value));
            if (matches > 1)
            {
                isValid = false;
            }
            return isValid;
        }

        private Boolean isColValid(int colIndex, String value)
        {
            Boolean isValid = true;
            int matches = playableGrid.Children.Cast<UIElement>().OfType<TextBox>().Count<TextBox>(e => Grid.GetColumn(e) == colIndex && e.Text.Equals(value));
            if (matches > 1)
            {
                isValid = false;
            }
            return isValid;
        }

        //method to determine if the same number already exists in the cell's 3x3 section
        private Boolean isSectionValid(int rowIndex, int colIndex, String value)
        {
            Boolean isValid = true;
            Section currentSection = determineSection(rowIndex, colIndex);

            int matches = playableGrid.Children.Cast<UIElement>().OfType<TextBox>().Count<TextBox>(e => Grid.GetRow(e) >= currentSection.lowerBoundRowIndex && Grid.GetRow(e) <= currentSection.upperBoundRowIndex && Grid.GetColumn(e) >= currentSection.lowerBoundColIndex && Grid.GetColumn(e) <= currentSection.upperBoundColIndex && e.Text.Equals(value));
            if (matches > 1)
            {
                isValid = false;
            }
            return isValid;
        }

        private class Section
        {
            public int sectionId { get; set; }
            public int lowerBoundRowIndex { get; set; }
            public int upperBoundRowIndex { get; set; }
            public int lowerBoundColIndex { get; set; }
            public int upperBoundColIndex { get; set; }
            public Boolean isCurrent { get; set; }

            public Section(int sectionId, int lowerBoundRowIndex, int upperBoundRowIndex, int lowerBoundColIndex, int upperBoundColIndex)
            {
                this.sectionId = sectionId;
                this.lowerBoundRowIndex = lowerBoundRowIndex;
                this.upperBoundRowIndex = upperBoundRowIndex;
                this.lowerBoundColIndex = lowerBoundColIndex;
                this.upperBoundColIndex = upperBoundColIndex;
            }

        }

        private Boolean populatePlayableGrid()
        {

            List<TextBox> textBoxList = playableGrid.Children.Cast<UIElement>().OfType<TextBox>().ToList();
            foreach (TextBox currentTextBox in textBoxList)
            {
                Boolean skip = false;

                List<int> validValueList = findValidValue(currentTextBox);

                //Check if no valid value exists. If not, shift the numbers to make it possible
                if (validValueList.Count == 0)
                {
                    List<TextBox> currentRowTextBoxes = playableGrid.Children.Cast<UIElement>().OfType<TextBox>().Where<TextBox>(e => Grid.GetRow(e) == Grid.GetRow(currentTextBox) && Grid.GetColumn(e) < Grid.GetColumn(currentTextBox)).ToList();
                    foreach (TextBox AnotherTextBox in currentRowTextBoxes)
                    {
                        List<int> AnotherValidValueList = findValidValue(AnotherTextBox);
                        if (AnotherValidValueList.Count > 1)
                        {
                            int anotherValue = Int32.Parse(AnotherTextBox.Text);
                            AnotherValidValueList.Remove(anotherValue);
                            currentTextBox.Text = AnotherTextBox.Text;
                            AnotherTextBox.Text = AnotherValidValueList.First<int>().ToString();
                            if (isMoveValid(currentTextBox))
                            {
                                skip = true;
                                break;
                            }
                            else
                            {
                                AnotherTextBox.Text = currentTextBox.Text;
                                currentTextBox.Clear();
                            }
                        }

                    }

                    if (!skip)
                    {
                        // if code execution reaches here, then a solution cannot be found. Need to try afresh
                        clearGrid();
                        return false;
                    }
                }
                if (skip)
                {
                    continue;
                }

                int randomIndex = rand.Next(validValueList.Count - 1);
                currentTextBox.Text = validValueList.ElementAt(randomIndex).ToString();

                //debug
                Debug.WriteLine("Current Textbox Col: " + Grid.GetColumn(currentTextBox));
                Debug.WriteLine("Current Textbox Row: " + Grid.GetRow(currentTextBox));
                Debug.WriteLine("Current Textbox Value: " + currentTextBox.Text);

            }

            return true;


        }

        private List<int> findValidValue(TextBox textBox)
        {
            String originalValue = textBox.Text;
            List<int> validValueList = new List<int>();
            for (int i = 1; i <= 9; i++)
            {
                textBox.Text = i.ToString();
                if (isMoveValid(textBox))
                {
                    validValueList.Add(i);
                }
            }

            textBox.Text = originalValue;

            return validValueList;
        }

        private void clearGrid()
        {
            List<TextBox> allTextBoxesList = playableGrid.Children.Cast<UIElement>().OfType<TextBox>().ToList();
            foreach (TextBox textBox in allTextBoxesList)
            {
                textBox.Clear();
            }
        }

        private void setUpGame()
        {
            Boolean gridIsPopulated = false;
            int myTrys = 0;
            int maxTrys = 5;

            while (!gridIsPopulated && myTrys < maxTrys)
            {
                gridIsPopulated = populatePlayableGrid();
                Debug.WriteLine("gridIsPopulated: " + gridIsPopulated + " on try " + myTrys);
                myTrys++;
            }

            Debug.WriteLine("Grid Populated Successfully!");

            removePlayableValues();

            Debug.WriteLine("Game Setup Complete");
        }

        private void removePlayableValues()
        {
            List<TextBox> allTextBoxesList = playableGrid.Children.Cast<UIElement>().OfType<TextBox>().ToList();
            int startIndex = rand.Next(allTextBoxesList.Count - 1);

            for (int i = startIndex; i < allTextBoxesList.Count; i++)
            {
                TextBox currentTextBox = allTextBoxesList.ElementAt(i);
                if (valueIsRemovable(currentTextBox))
                {
                    currentTextBox.Clear();
                    currentTextBox.TextChanged += new TextChangedEventHandler(numberEntered);
                    currentTextBox.IsReadOnly = false;
                    numOfEditableTextBoxes++;
                }
                else
                {
                    currentTextBox.IsReadOnly = true;
                }
            }

            for (int i = startIndex-1; i >= 0; i--)
            {
                TextBox currentTextBox = allTextBoxesList.ElementAt(i);
                if (valueIsRemovable(currentTextBox))
                {
                    currentTextBox.Clear();
                    currentTextBox.TextChanged += new TextChangedEventHandler(numberEntered);
                    currentTextBox.IsReadOnly = false;
                    numOfEditableTextBoxes++;
                }
                else
                {
                    currentTextBox.IsReadOnly = true;
                }
            }

            Debug.WriteLine("Playable Values Removed");
        }

        private Boolean valueIsRemovable(TextBox textBox)
        {
            Boolean isRemovable = false;

            List<int> validValueList = findValidValue(textBox);
            if(validValueList.Count == 1)
            {
                isRemovable = true;
            }

            return isRemovable;
        }

        private void runWinCondition()
        {
            MessageBox.Show("You Win!!!");
            Page homePage = new HomePage();
            Window.GetWindow(this).Content = homePage;
        }

        private void loadGame()
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                string gameStateString = System.IO.File.ReadAllText(filename);
                //TODO: Add parsing and setup game for loading
            }
        }
    }
}