using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ImageClassifier_OpenCV
{
    public partial class ImageForm : Form
    {
        private String nCurrentPicture;
        private Point tStartPos = new Point();
        private Point tCurPos = new Point();
        private Image imgBase;
        private Image imgFore;
        private bool drawing;

        public Rectangle tRect;
        public Rectangle tOldRect;
        
        private Rectangle getRectangle()
        {
            return new Rectangle(
                Math.Min(tStartPos.X, tCurPos.X),
                Math.Min(tStartPos.Y, tCurPos.Y),
                Math.Abs(tStartPos.X - tCurPos.X),
                Math.Abs(tStartPos.Y - tCurPos.Y));
        }

        public ImageForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            pbPicture.MouseUp += new MouseEventHandler(ImageForm_MouseUp);
            pbPicture.MouseDown += new MouseEventHandler(ImageForm_MouseDown);
            pbPicture.MouseMove += new MouseEventHandler(ImageForm_MouseMove);
            pbPicture.Paint += new PaintEventHandler(imageForm_paint);
            drawing = false;
            //fImg.MouseUp += new MouseEventHandler(fImg_MouseUp);
        }

        void ImageForm_MouseDown(object sender, MouseEventArgs e)
        {
            tCurPos = tStartPos = e.Location;
            tRect = getRectangle();
            drawing = true;
        }

        void ImageForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (drawing)
            {
                tCurPos = e.Location;
                tRect = getRectangle();
                pbPicture.Invalidate();
            }
        }

        void ImageForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (drawing)
            {
                drawing = false;
                tRect = getRectangle();
                pbPicture.Invalidate();
            }
        }

        private void ImageForm_Load(object sender, EventArgs e)
        {
        }

        public void ShowPicture(DataRow drCurrent)
        {
            nCurrentPicture = Convert.ToString(drCurrent["Filename"]);
            imgBase = Image.FromFile(nCurrentPicture);
            imgFore = (Image)imgBase.Clone();
            pbPicture.Image = imgFore;
            this.Size = new Size(imgBase.Width + 6, imgBase.Height + 14);
            drawing = false;
        }

        private void imageForm_paint(object sender, EventArgs e)
        {
            if ((false == tRect.Location.Equals(tOldRect.Location)) ||
                (false == tRect.Size.Equals(tOldRect.Size)))
            {
                pbPicture.Image = (Image)imgBase.Clone();

                if (null != pbPicture.Image)
                {
                    using (Graphics g = Graphics.FromImage(pbPicture.Image))
                    {
                        g.DrawRectangle(Pens.Red, tRect);
                    }
                }

                tOldRect.Location = tRect.Location;
                tOldRect.Size = tRect.Size;
            }
        }
    }
}
