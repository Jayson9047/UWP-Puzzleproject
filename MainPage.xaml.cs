/*
* FILE : MainPage.xaml.cs
* PROJECT : Puzzle
* PROGRAMMER :  Jayson Ovishek Biswas
* UPDATED VERSION : 2019-11-29
* DESCRIPTION : This UWP application demonstrates a shuffle puzzle game. THe game takes an image and 
*               cuts it into a 4X4 matrix. Then it saves the combination and shuffles the images. 
*               If the player manages to get the right combination back by moving the images' position, he wins.
*               Here the players can choose photos from their own computer.They can also take photos from the webcam 
*               and use it in the game to cut and shuffle. There is a timer that is started as soon as the user chooses
*               an image. The game gives the user 2 minutes to finish it. If the user can't finish it in two minutes,
*               he loses. 
*/

using System;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Puzzle
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 


    public sealed partial class MainPage : Page
    {
        DispatcherTimer Timer1;             //the timer
        private int time = 120;             //2 minutes is given
        private const int GameTime = 120;   //2 minutes is given
        


        Button btn1 = new Button();         
        bool play = false;              //check if game is running or not
        public MainPage()
        {
            this.InitializeComponent();
        }

        Image[,] images = new Image[4, 4];          //combination after shuffling
        Image[,] winCombo = new Image[4, 4];        //winning combination


        //Lets the user choose an image from the local drive. If an image is chosen, a timer is started. 
        //Also, this method calls the PlaceImg method to cut and shuffle.
        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            loser.Visibility = Visibility.Collapsed;
            winner.Visibility = Visibility.Collapsed;
            // Set up the file picker.
            Windows.Storage.Pickers.FileOpenPicker openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;

            // Filter to include a sample subset of file types.
            openPicker.FileTypeFilter.Clear();
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".jpg");
            Error.Visibility = Visibility.Collapsed;
            // Open the file picker.
            Windows.Storage.StorageFile file = await openPicker.PickSingleFileAsync();

            // 'file' is null if user cancels the file picker.
            if (file != null)
            {
                // Open a stream for the selected file.
                // The 'using' block ensures the stream is disposed
                // after the image is loaded.
                using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                    await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    // Set the image source to the selected bitmap.
                    Windows.UI.Xaml.Media.Imaging.BitmapImage bitmapImage =
                        new Windows.UI.Xaml.Media.Imaging.BitmapImage();

                    bitmapImage.SetSource(fileStream);
                    image1.Source = bitmapImage;
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                    PlaceImg(decoder);
                }
                play = true;
                Timer1 = new Windows.UI.Xaml.DispatcherTimer();
                Timer1.Interval = TimeSpan.FromMilliseconds(1000);
                Timer1.Tick += Timer1_Tick;
                if (Timer1.IsEnabled)
                {
                    Timer1.Stop();
                }
                Timer1.Start();
                time = GameTime;

                Choose.IsEnabled = false;
                Capture.IsEnabled = false;
            }
        }




        //This method cuts the picture into 4X4 matrix. Then initializes the winCombo and calls Shuffle method to shuffle the images.
        private async void PlaceImg(BitmapDecoder decoder)
        {
            var windowHeight = ((Frame)Window.Current.Content).ActualHeight;
            var windowWidth = ((Frame)Window.Current.Content).ActualWidth;
            var imgHeight = decoder.PixelHeight / 4;
            var imgWidth = decoder.PixelWidth / 4;


            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    winCombo[i, j] = new Image();
                    images[i, j] = new Image();
                    //Image img01 = new Image();
                    //if ((i==3) && (j==3))
                    //{
                    //    img01.Source = null;
                    //    continue;
                    //}
                    InMemoryRandomAccessStream ras = new InMemoryRandomAccessStream(); //memory for loading the image into the encoder
                    BitmapEncoder enc = await BitmapEncoder.CreateForTranscodingAsync(ras, decoder); //bitmap encoder ini
                    BitmapBounds bounds = new BitmapBounds(); //used to tranform the encoder to specify which part of the image will be used
                    bounds.Height = imgHeight;
                    bounds.Width = imgWidth;
                    bounds.X = 0 + imgWidth * (uint)i;
                    bounds.Y = 0 + imgHeight * (uint)j;
                    enc.BitmapTransform.Bounds = bounds;   //to tell the encoder how the cutting will be done
                    try
                    {
                        await enc.FlushAsync(); //actual cutting
                    }
                    catch (Exception ex)
                    {
                        string s = ex.ToString();
                    }

                    BitmapImage bImg = new BitmapImage(); //an image to be displayed

                    bImg.SetSource(ras);
                    //img01.Source = bImg;
                    //img01.HorizontalAlignment = HorizontalAlignment.Stretch;
                    //img01.VerticalAlignment = VerticalAlignment.Stretch;

                    //Grid.SetRow(img01, j);
                    //Grid.SetColumn(img01, i);

                    winCombo[i, j].Source = bImg;

                    //cut the bottom corner of the picture.
                    if ((i == 3) && (j == 3))
                    {
                        winCombo[i, j].Source = null;
                    }


                    //myGrid.Children.Add(img01);
                }
            }
            Shuffle();
        }

        //This Method shuffles the images and places them into different cells of the grid.
        private void Shuffle()
        {

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Random rnd = new Random();
                    int column = rnd.Next(4);
                    int row = rnd.Next(4);

                    //if a cell is null, fill it up with image
                    if (images[column, row].Source == null)
                    {
                        images[column, row].Source = winCombo[i, j].Source;

                    }
                    else
                    {
                        //if a cell image is not null, then keep looking for null and after finding, place the image

                        while (images[column, row].Source != null)
                        {
                            column = rnd.Next(4);
                            row = rnd.Next(4);
                        }

                        images[column, row].Source = winCombo[i, j].Source;
                    }
                    putItIn(column, row);
                }
            }

        }


        // Put all the images in the right cell of the grid.
        //Parameter: int    i    indicates the number of column
        //           int    j    indicates the number of row
        private void putItIn(int i, int j)
        {
            Button btn = new Button();
            bool empty = false;
            if (images[i, j].Source == null)
            {
                empty = true;
            }

            if ((i == 0) && (j == 0))
            {
                img00.Source = images[i, j].Source;

                btn = btn00;

            }

            if ((i == 0) && (j == 1))
            {
                img01.Source = images[i, j].Source;
                btn = btn01;
            }
            if ((i == 0) && (j == 2))
            {
                img02.Source = images[i, j].Source;
                btn = btn02;
            }
            if ((i == 0) && (j == 3))
            {
                img03.Source = images[i, j].Source;
                btn = btn03;
            }
            if ((i == 1) && (j == 0))
            {
                img10.Source = images[i, j].Source;
                btn = btn10;
            }
            if ((i == 1) && (j == 1))
            {
                img11.Source = images[i, j].Source;
                btn = btn11;
            }
            if ((i == 1) && (j == 2))
            {
                img12.Source = images[i, j].Source;
                btn = btn12;
            }
            if ((i == 1) && (j == 3))
            {
                img13.Source = images[i, j].Source;
                btn = btn13;
            }
            if ((i == 2) && (j == 0))
            {
                img20.Source = images[i, j].Source;
                btn = btn20;
            }
            if ((i == 2) && (j == 1))
            {
                img21.Source = images[i, j].Source;
                btn = btn21;
            }
            if ((i == 2) && (j == 2))
            {
                img22.Source = images[i, j].Source;
                btn = btn22;
            }
            if ((i == 2) && (j == 3))
            {
                img23.Source = images[i, j].Source;
                btn = btn23;
            }
            if ((i == 3) && (j == 0))
            {
                img30.Source = images[i, j].Source;
                btn = btn30;
            }
            if ((i == 3) && (j == 1))
            {
                img31.Source = images[i, j].Source;
                btn = btn31;
            }
            if ((i == 3) && (j == 2))
            {
                img32.Source = images[i, j].Source;
                btn = btn32;
            }
            if ((i == 3) && (j == 3))
            {
                img33.Source = images[i, j].Source;
                btn = btn33;
            }

            //if the image is null, this means we've reached the last block for shuffling. So, one block should be empty in order to move others
            if (empty == true)
            {
                btn1 = btn;
            }
            //move the button to the specified column and row
            Grid.SetColumn(btn, i);
            Grid.SetRow(btn, j);
        }
        private StorageFile storeFile;
        private IRandomAccessStream stream;

        //This Method takes picture of the user and creates a new game with the picture
        private async void captureBtn_Click(object sender, RoutedEventArgs e)
        {
            loser.Visibility = Visibility.Collapsed;
            winner.Visibility = Visibility.Collapsed;
            CameraCaptureUI capture = new CameraCaptureUI();
            capture.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            capture.PhotoSettings.CroppedAspectRatio = new Size(3, 5);
            capture.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.HighestAvailable;
            storeFile = await capture.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (storeFile != null)
            {
                BitmapImage bimage = new BitmapImage();
                stream = await storeFile.OpenAsync(FileAccessMode.Read);
                bimage.SetSource(stream);
                image1.Source = bimage;
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                PlaceImg(decoder);
                play = true;

                Timer1 = new Windows.UI.Xaml.DispatcherTimer();
                Timer1.Interval = TimeSpan.FromMilliseconds(1000);
                Timer1.Tick += Timer1_Tick;
                if (Timer1.IsEnabled)
                {
                    Timer1.Stop();
                }
                Timer1.Start();
                time = GameTime;
                Choose.IsEnabled = false;
                Capture.IsEnabled = false;
            }
        }



        // This method deternmines the movements of the blocks and images. An image or button can only move if it has a null nearby. 
        // Otherwise, the button can't be moved. Every time any button from the matrix is clicked, this method is called. And everytime the 
        //method is called, it checks if the user has got the winning combination or not. 
        private void btn00_Click(object sender, RoutedEventArgs e)
        {

            Button btn = new Button();
            btn = sender as Button;
            int row = Grid.GetRow(btn);
            int column = Grid.GetColumn(btn);

            if (play == true)
            {
                if ((column == 3) && (row == 3))
                {
                    if (images[3, 2].Source == null)
                    {
                        Reposition(btn, 3, 2, column, row);


                    }
                    if (images[2, 3].Source == null)
                    {
                        Reposition(btn, 2, 3, column, row);


                    }
                }
                else if ((column == 0) && (row == 0))
                {
                    if (images[0, 1].Source == null)
                    {
                        Reposition(btn, 0, 1, column, row);
                    }
                    if (images[1, 0].Source == null)
                    {
                        Reposition(btn, 1, 0, column, row);
                    }
                }

                else if ((column == 3) && (row == 0))
                {
                    if (images[2, 0].Source == null)
                    {
                        Reposition(btn, 2, 0, column, row);
                    }
                    if (images[3, 1].Source == null)
                    {
                        Reposition(btn, 3, 1, column, row);
                    }
                }

                else if ((column == 0) && (row == 3))
                {
                    if (images[0, 2].Source == null)
                    {
                        Reposition(btn, 0, 2, column, row);
                    }
                    if (images[1, 3].Source == null)
                    {
                        Reposition(btn, 1, 3, column, row);
                    }
                }
                else if ((row == 3) && (column < 3))
                {
                    if (images[column, (row - 1)].Source == null)
                    {
                        Reposition(btn, column, (row - 1), column, row);


                    }
                    if (images[(column - 1), row].Source == null)
                    {
                        Reposition(btn, (column - 1), row, column, row);


                    }
                    if (images[(column + 1), row].Source == null)
                    {
                        Reposition(btn, (column + 1), row, column, row);


                    }
                }

                else if ((column == 3) && (row < 3))
                {
                    if (images[column, (row - 1)].Source == null)
                    {
                        Reposition(btn, column, (row - 1), column, row);
                    }
                    if (images[(column - 1), row].Source == null)
                    {
                        Reposition(btn, (column - 1), row, column, row);
                    }
                    if (images[column, (row + 1)].Source == null)
                    {
                        Reposition(btn, column, (row + 1), column, row);
                    }
                }

                else if ((column == 0) && (row < 3))
                {
                    if (images[column, (row - 1)].Source == null)
                    {
                        Reposition(btn, column, (row - 1), column, row);
                    }
                    if (images[(column + 1), row].Source == null)
                    {
                        Reposition(btn, (column + 1), row, column, row);
                    }
                    if (images[column, (row + 1)].Source == null)
                    {
                        Reposition(btn, column, (row + 1), column, row);
                    }
                }
                else if ((row == 0) && (column < 3))
                {
                    if (images[column, (row + 1)].Source == null)
                    {
                        Reposition(btn, column, (row + 1), column, row);
                    }
                    if (images[(column - 1), row].Source == null)
                    {
                        Reposition(btn, (column - 1), row, column, row);
                    }
                    if (images[(column + 1), row].Source == null)
                    {
                        Reposition(btn, (column + 1), row, column, row);
                    }
                }
                else if (((row == 1) && (column == 2)) || ((row == 1) && (column == 1)))
                {
                    if (images[column, (row + 1)].Source == null)
                    {
                        Reposition(btn, column, (row + 1), column, row);
                    }
                    if (images[(column - 1), row].Source == null)
                    {
                        Reposition(btn, (column - 1), row, column, row);
                    }
                    if (images[(column + 1), row].Source == null)
                    {
                        Reposition(btn, (column + 1), row, column, row);
                    }
                    if (images[column, (row - 1)].Source == null)
                    {
                        Reposition(btn, column, (row - 1), column, row);
                    }
                }

                else if (((row == 2) && (column == 1)) || ((row == 2) && (column == 2)))
                {
                    if (images[column, (row + 1)].Source == null)
                    {
                        Reposition(btn, column, (row + 1), column, row);

                    }
                    if (images[(column - 1), row].Source == null)
                    {
                        Reposition(btn, (column - 1), row, column, row);
                    }
                    if (images[(column + 1), row].Source == null)
                    {
                        Reposition(btn, (column + 1), row, column, row);

                    }
                    if (images[column, (row - 1)].Source == null)
                    {
                        Reposition(btn, column, (row - 1), column, row);

                    }
                }


            }
            else
            {
                Error.Visibility = Visibility.Visible;
            }

            CheckWin();
        }

        //If the user has got the winnning combo, the he has won. And a Victory message will be presented. Otherwise, after the timer
        //hits zero, the player loses.
        //
        private void CheckWin()
        {
            bool win = false;
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (images[i, j].Source != winCombo[i, j].Source)
                        {
                            win = false;
                        }
                    }
                }


                if (win == true)
                {
                    winner.Visibility = Visibility.Visible;
                    Timer1.Stop();
                    play = false;
                }
            }
            catch (Exception)
            {
                Error.Visibility = Visibility.Visible;
            }

        }

        //This method is used to change the position of the buttone sending the requests.

        private void Reposition(FrameworkElement element, int tarColumn, int tarRow, int column, int row)
        {

            Grid.SetRow(element, tarRow);
            Grid.SetColumn(element, tarColumn);

            images[tarColumn, tarRow].Source = images[column, row].Source;
            images[column, row].Source = null;
            Grid.SetRow(btn1, row);
            Grid.SetColumn(btn1, column);

        }

        //This method counts down from 120 to zero. If the time is up before the user has got the winning combo, then the player loses.
        // And game stops.
        private void Timer1_Tick(object sender, object e)
        {
            time--;
            int minutes = time / 60;
            Timer.Text = "00:0" + minutes.ToString() + ":" + (time % 60).ToString();
            if (time > 0)
            {
                if (time <= 30)         //Start blinking at the point of 30 seconds
                {
                    if (time % 2 == 0)
                    {
                        Timer.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        Timer.Foreground = new SolidColorBrush(Colors.Aqua);
                    }
                }
            }
            else
            {
                play = false;
                Timer1.Stop();
                loser.Visibility = Visibility.Visible;
                Choose.IsEnabled = true;
                Capture.IsEnabled = true;
                Timer.Foreground = new SolidColorBrush(Colors.Brown);
            }
        }


        //The images are set in place and the player automatically wins on clicking this button
        private void win_Click(object sender, RoutedEventArgs e)
        {
            Button btn = new Button();
            btn = sender as Button; 
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    images[i, j].Source = winCombo[i, j].Source;
                    putItIn(i, j);
                    winner.Visibility = Visibility.Visible;
                }
            }

            Timer1.Stop();
            Choose.IsEnabled = true;
            Capture.IsEnabled = true;

        }
    }
}
