using System;

using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using McuCommunication.Commodules;

namespace EscInstaller.Converters
{
    public class PointsCnvtr : BaseConverter, IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public PointsCnvtr() {}

        public object Convert(object[] value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            int a;
            LinkTo b;
            bool c;
            bool d;

            if (int.TryParse(value[0].ToString(), out a) && Enum.TryParse(value[1].ToString(), out b) &&
                bool.TryParse(value[2].ToString(), out c) && bool.TryParse(value[3].ToString(), out d))
                return RouteLine(a, b, c, d);
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Draw line according to routing strategy
        /// </summary>
        /// <param name="id"> flow id </param>
        /// <param name="linkTo"> routing type </param>
        /// <param name="singleDelayBlock"> if the first delay block uses all delay space available </param>
        /// <param name="hasBackup">If no amplifier, shorter line </param>
        /// <returns> </returns>
        private static PointCollection RouteLine(int id, LinkTo linkTo, bool singleDelayBlock, bool hasBackup)
        {
            if (id > 499) return new PointCollection();
            var flowNdumber = id % 12;
            var y = flowNdumber % 4;

            int[] ypos = { 25, 65, 105, 145, 185 };
            int[] xpos = { 130, 240, 270, 390, 455 };
            int length = hasBackup ? 1000 : 860;

            if (linkTo == LinkTo.No)
                return new PointCollection
                           {
                               new Point(260, ypos[y]),
                               new Point(length, ypos[y])
                           };

            if (flowNdumber == 2 && linkTo == LinkTo.PreviousWithDelay && !singleDelayBlock)
                return new PointCollection
                           {
                               new Point(xpos[4], ypos[1]),
                               //from upper linkunit
                               new Point(xpos[4], 80), //go down half
                               new Point(xpos[3], 80), //go left one unit 
                               new Point(xpos[3], ypos[2]), //go down half
                               new Point(length, ypos[2]), //go to end
                               //go to end
                           };

            if ((linkTo == LinkTo.Previous && flowNdumber > 0 && flowNdumber < 3) ||
                ((linkTo == LinkTo.Previous && flowNdumber == 3 && !singleDelayBlock)))
                return new PointCollection
                           {
                               new Point(xpos[3], ypos[y - 1]), //from upper linkunit
                               new Point(xpos[3], ypos[y]), //go down & attach cards
                               new Point(length, ypos[y]) //go to end
                           };

            if (linkTo == LinkTo.PreviousWithDelay || linkTo == LinkTo.Previous)

                return new PointCollection
                           {
                               (y == 0)
                                   ? new Point(xpos[4], -15)
                                   : new Point(xpos[4], ypos[y - 1]), //from upper linkunit
                               new Point(xpos[4], ypos[y]), //go down 1
                               new Point(length, ypos[y]) //go to end
                           };


            throw new Exception("No valid routing path found");
        }

        #endregion
    }
}