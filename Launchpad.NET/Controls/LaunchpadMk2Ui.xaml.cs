using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Launchpad.NET.Models;
using System.Reactive.Linq;
using System.Threading;
using Windows.Devices.Midi;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Launchpad.NET.Controls
{
    public sealed partial class LaunchpadMk2Ui
    {
        /// <summary>
        /// The launchpad this control is bound to
        /// </summary>
        public static readonly DependencyProperty LaunchpadMk2Property =
            DependencyProperty.Register(
                "LaunchpadMk2", typeof(LaunchpadMk2),
                typeof(LaunchpadMk2Ui), null
            );

        // The property wrapper, so that callers can use this property through a simple ExampleClassInstance.IsSpinning usage rather than requiring property system APIs
        public LaunchpadMk2 LaunchpadMk2
        {
            get { return (LaunchpadMk2)GetValue(LaunchpadMk2Property); }
            set
            {
                SetValue(LaunchpadMk2Property, value);

                UpdateButtons();

                value
                    .WhenButtonStateChanged
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(_ => 
                    {
                        Bindings.Update();
                    });
            }
        }

        public LaunchpadMk2TopButton Top104, Top105, Top106, Top107, Top108, Top109, Top110, Top111;

        public LaunchpadMk2Button Grid00, Grid10, Grid20, Grid30, Grid40, Grid50, Grid60, Grid70,
                                  Grid01, Grid11, Grid21, Grid31, Grid41, Grid51, Grid61, Grid71,
                                  Grid02, Grid12, Grid22, Grid32, Grid42, Grid52, Grid62, Grid72,
                                  Grid03, Grid13, Grid23, Grid33, Grid43, Grid53, Grid63, Grid73,
                                  Grid04, Grid14, Grid24, Grid34, Grid44, Grid54, Grid64, Grid74,
                                  Grid05, Grid15, Grid25, Grid35, Grid45, Grid55, Grid65, Grid75,
                                  Grid06, Grid16, Grid26, Grid36, Grid46, Grid56, Grid66, Grid76,
                                  Grid07, Grid17, Grid27, Grid37, Grid47, Grid57, Grid67, Grid77;

        public LaunchpadMk2Button Side70, Side71, Side72, Side73, Side74, Side75, Side76, Side77;

        public LaunchpadMk2Ui()
        {
            this.InitializeComponent();

            BuildInteractions();
        }

        void BuildInteractions()
        {
            RpButton104.Tapped += (s, e) => SimulatePress(104);
            RpButton105.Tapped += (s, e) => SimulatePress(105);
            RpButton106.Tapped += (s, e) => SimulatePress(106);
            RpButton107.Tapped += (s, e) => SimulatePress(107);
            RpButton108.Tapped += (s, e) => SimulatePress(108);
            RpButton109.Tapped += (s, e) => SimulatePress(109);
            RpButton110.Tapped += (s, e) => SimulatePress(110);
            RpButton111.Tapped += (s, e) => SimulatePress(111);

            RpButton00.Tapped += (s, e) => SimulatePress(11);
            RpButton10.Tapped += (s, e) => SimulatePress(12);
            RpButton20.Tapped += (s, e) => SimulatePress(13);
            RpButton30.Tapped += (s, e) => SimulatePress(14);
            RpButton40.Tapped += (s, e) => SimulatePress(15);
            RpButton50.Tapped += (s, e) => SimulatePress(16);
            RpButton60.Tapped += (s, e) => SimulatePress(17);
            RpButton70.Tapped += (s, e) => SimulatePress(18);
            RpButton01.Tapped += (s, e) => SimulatePress(21);
            RpButton11.Tapped += (s, e) => SimulatePress(22);
            RpButton21.Tapped += (s, e) => SimulatePress(23);
            RpButton31.Tapped += (s, e) => SimulatePress(24);
            RpButton41.Tapped += (s, e) => SimulatePress(25);
            RpButton51.Tapped += (s, e) => SimulatePress(26);
            RpButton61.Tapped += (s, e) => SimulatePress(27);
            RpButton71.Tapped += (s, e) => SimulatePress(28);
            RpButton02.Tapped += (s, e) => SimulatePress(31);
            RpButton12.Tapped += (s, e) => SimulatePress(32);
            RpButton22.Tapped += (s, e) => SimulatePress(33);
            RpButton32.Tapped += (s, e) => SimulatePress(34);
            RpButton42.Tapped += (s, e) => SimulatePress(35);
            RpButton52.Tapped += (s, e) => SimulatePress(36);
            RpButton62.Tapped += (s, e) => SimulatePress(37);
            RpButton72.Tapped += (s, e) => SimulatePress(38);
            RpButton03.Tapped += (s, e) => SimulatePress(41);
            RpButton13.Tapped += (s, e) => SimulatePress(42);
            RpButton23.Tapped += (s, e) => SimulatePress(43);
            RpButton33.Tapped += (s, e) => SimulatePress(44);
            RpButton43.Tapped += (s, e) => SimulatePress(45);
            RpButton53.Tapped += (s, e) => SimulatePress(46);
            RpButton63.Tapped += (s, e) => SimulatePress(47);
            RpButton73.Tapped += (s, e) => SimulatePress(48);
            RpButton04.Tapped += (s, e) => SimulatePress(51);
            RpButton14.Tapped += (s, e) => SimulatePress(52);
            RpButton24.Tapped += (s, e) => SimulatePress(53);
            RpButton34.Tapped += (s, e) => SimulatePress(54);
            RpButton44.Tapped += (s, e) => SimulatePress(55);
            RpButton54.Tapped += (s, e) => SimulatePress(56);
            RpButton64.Tapped += (s, e) => SimulatePress(57);
            RpButton74.Tapped += (s, e) => SimulatePress(58);
            RpButton05.Tapped += (s, e) => SimulatePress(61);
            RpButton15.Tapped += (s, e) => SimulatePress(62);
            RpButton25.Tapped += (s, e) => SimulatePress(63);
            RpButton35.Tapped += (s, e) => SimulatePress(64);
            RpButton45.Tapped += (s, e) => SimulatePress(65);
            RpButton55.Tapped += (s, e) => SimulatePress(66);
            RpButton65.Tapped += (s, e) => SimulatePress(67);
            RpButton75.Tapped += (s, e) => SimulatePress(68);
            RpButton06.Tapped += (s, e) => SimulatePress(71);
            RpButton16.Tapped += (s, e) => SimulatePress(72);
            RpButton26.Tapped += (s, e) => SimulatePress(73);
            RpButton36.Tapped += (s, e) => SimulatePress(74);
            RpButton46.Tapped += (s, e) => SimulatePress(75);
            RpButton56.Tapped += (s, e) => SimulatePress(76);
            RpButton66.Tapped += (s, e) => SimulatePress(77);
            RpButton76.Tapped += (s, e) => SimulatePress(78);
            RpButton07.Tapped += (s, e) => SimulatePress(81);
            RpButton17.Tapped += (s, e) => SimulatePress(82);
            RpButton27.Tapped += (s, e) => SimulatePress(83);
            RpButton37.Tapped += (s, e) => SimulatePress(84);
            RpButton47.Tapped += (s, e) => SimulatePress(85);
            RpButton57.Tapped += (s, e) => SimulatePress(86);
            RpButton67.Tapped += (s, e) => SimulatePress(87);
            RpButton77.Tapped += (s, e) => SimulatePress(88);

            RpSide70.Tapped += (s, e) => SimulatePress(19);
            RpSide71.Tapped += (s, e) => SimulatePress(29);
            RpSide72.Tapped += (s, e) => SimulatePress(39);
            RpSide73.Tapped += (s, e) => SimulatePress(49);
            RpSide74.Tapped += (s, e) => SimulatePress(59);
            RpSide75.Tapped += (s, e) => SimulatePress(69);
            RpSide76.Tapped += (s, e) => SimulatePress(79);
            RpSide77.Tapped += (s, e) => SimulatePress(89);
        }

        void SimulatePress(int id)
        {
            if (id >= 104)
            {
                LaunchpadMk2?.SimulateTopPress(id);
                LaunchpadMk2?.SimulateTopRelease(id);
            }
            else
            {
                if (id % 10 == 9)
                {
                    //UpdateSideButton(new MidiNoteOnMessage(0, (byte)id, 127));
                }
                else
                {
                    LaunchpadMk2?.SimulateGridPress(id);
                    LaunchpadMk2?.SimulateGridRelease(id);
                }
            }
        }

        /// <summary>
        /// Binds the Launchpad Ui control to the values of a LaunchpadMk2 model
        /// </summary>
        void UpdateButtons()
        {
            // Bind to all the top buttons
            if (LaunchpadMk2?.TopButtons != null)
            {
                Top104 = LaunchpadMk2.TopButtons[0];
                Top105 = LaunchpadMk2.TopButtons[1];
                Top106 = LaunchpadMk2.TopButtons[2];
                Top107 = LaunchpadMk2.TopButtons[3];
                Top108 = LaunchpadMk2.TopButtons[4];
                Top109 = LaunchpadMk2.TopButtons[5];
                Top110 = LaunchpadMk2.TopButtons[6];
                Top111 = LaunchpadMk2.TopButtons[7];
            }

            // Bind to the grid buttons
            if (LaunchpadMk2?.Grid != null)
            {
                Grid00 = LaunchpadMk2.Grid[0, 0];
                Grid10 = LaunchpadMk2.Grid[1, 0];
                Grid20 = LaunchpadMk2.Grid[2, 0];
                Grid30 = LaunchpadMk2.Grid[3, 0];
                Grid40 = LaunchpadMk2.Grid[4, 0];
                Grid50 = LaunchpadMk2.Grid[5, 0];
                Grid60 = LaunchpadMk2.Grid[6, 0];
                Grid70 = LaunchpadMk2.Grid[7, 0];
                Grid01 = LaunchpadMk2.Grid[0, 1];
                Grid11 = LaunchpadMk2.Grid[1, 1];
                Grid21 = LaunchpadMk2.Grid[2, 1];
                Grid31 = LaunchpadMk2.Grid[3, 1];
                Grid41 = LaunchpadMk2.Grid[4, 1];
                Grid51 = LaunchpadMk2.Grid[5, 1];
                Grid61 = LaunchpadMk2.Grid[6, 1];
                Grid71 = LaunchpadMk2.Grid[7, 1];
                Grid02 = LaunchpadMk2.Grid[0, 2];
                Grid12 = LaunchpadMk2.Grid[1, 2];
                Grid22 = LaunchpadMk2.Grid[2, 2];
                Grid32 = LaunchpadMk2.Grid[3, 2];
                Grid42 = LaunchpadMk2.Grid[4, 2];
                Grid52 = LaunchpadMk2.Grid[5, 2];
                Grid62 = LaunchpadMk2.Grid[6, 2];
                Grid72 = LaunchpadMk2.Grid[7, 2];
                Grid03 = LaunchpadMk2.Grid[0, 3];
                Grid13 = LaunchpadMk2.Grid[1, 3];
                Grid23 = LaunchpadMk2.Grid[2, 3];
                Grid33 = LaunchpadMk2.Grid[3, 3];
                Grid43 = LaunchpadMk2.Grid[4, 3];
                Grid53 = LaunchpadMk2.Grid[5, 3];
                Grid63 = LaunchpadMk2.Grid[6, 3];
                Grid73 = LaunchpadMk2.Grid[7, 3];
                Grid04 = LaunchpadMk2.Grid[0, 4];
                Grid14 = LaunchpadMk2.Grid[1, 4];
                Grid24 = LaunchpadMk2.Grid[2, 4];
                Grid34 = LaunchpadMk2.Grid[3, 4];
                Grid44 = LaunchpadMk2.Grid[4, 4];
                Grid54 = LaunchpadMk2.Grid[5, 4];
                Grid64 = LaunchpadMk2.Grid[6, 4];
                Grid74 = LaunchpadMk2.Grid[7, 4];
                Grid05 = LaunchpadMk2.Grid[0, 5];
                Grid15 = LaunchpadMk2.Grid[1, 5];
                Grid25 = LaunchpadMk2.Grid[2, 5];
                Grid35 = LaunchpadMk2.Grid[3, 5];
                Grid45 = LaunchpadMk2.Grid[4, 5];
                Grid55 = LaunchpadMk2.Grid[5, 5];
                Grid65 = LaunchpadMk2.Grid[6, 5];
                Grid75 = LaunchpadMk2.Grid[7, 5];
                Grid06 = LaunchpadMk2.Grid[0, 6];
                Grid16 = LaunchpadMk2.Grid[1, 6];
                Grid26 = LaunchpadMk2.Grid[2, 6];
                Grid36 = LaunchpadMk2.Grid[3, 6];
                Grid46 = LaunchpadMk2.Grid[4, 6];
                Grid56 = LaunchpadMk2.Grid[5, 6];
                Grid66 = LaunchpadMk2.Grid[6, 6];
                Grid76 = LaunchpadMk2.Grid[7, 6];
                Grid07 = LaunchpadMk2.Grid[0, 7];
                Grid17 = LaunchpadMk2.Grid[1, 7];
                Grid27 = LaunchpadMk2.Grid[2, 7];
                Grid37 = LaunchpadMk2.Grid[3, 7];
                Grid47 = LaunchpadMk2.Grid[4, 7];
                Grid57 = LaunchpadMk2.Grid[5, 7];
                Grid67 = LaunchpadMk2.Grid[6, 7];
                Grid77 = LaunchpadMk2.Grid[7, 7];
            }

            // Bind to all the side buttons
            if (LaunchpadMk2?.SideButtons != null)
            {
                Side70 = LaunchpadMk2.SideButtons[0];
                Side71 = LaunchpadMk2.SideButtons[1];
                Side72 = LaunchpadMk2.SideButtons[2];
                Side73 = LaunchpadMk2.SideButtons[3];
                Side74 = LaunchpadMk2.SideButtons[4];
                Side75 = LaunchpadMk2.SideButtons[5];
                Side76 = LaunchpadMk2.SideButtons[6];
                Side77 = LaunchpadMk2.SideButtons[7];
            }

            Bindings.Update();
        }

    }
}
