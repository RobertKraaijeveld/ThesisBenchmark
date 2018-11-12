using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Benchmarking_Console_App.Configurations.Databases.Attributes;
using Benchmarking_program.Models.DatabaseModels;
using DateTime = System.DateTime;

namespace Benchmarking_Console_App.Models.DatabaseModels
{
    public class MinuteAveragesRow : AbstractModel<MinuteAveragesRow>
    {
        [IsPrimaryKey]
        public long MinuteAveragesRowId { get; set; }

        public double startid { get; set; }
        public double endid { get; set; }
        public double engine1 { get; set; }
        public double engine2 { get; set; }
        public double engine3 { get; set; }
        public double engine4 { get; set; }
        public double engine5 { get; set; }
        public double engine6 { get; set; }
        public double engine7 { get; set; }
        public double engine8 { get; set; }
        public double flow1 { get; set; }
        public double flow2 { get; set; }
        public double flow3 { get; set; }
        public double flow4 { get; set; }
        public double flow5 { get; set; }
        public double flow6 { get; set; }
        public double flow7 { get; set; }
        public double flow8 { get; set; }
        public double flow9 { get; set; }
        public double flow10 { get; set; }
        public double flow11 { get; set; }
        public double flow12 { get; set; }
        public double flowtemp1 { get; set; }
        public double flowtemp2 { get; set; }
        public double flowtemp3 { get; set; }
        public double flowtemp4 { get; set; }
        public double flowtemp5 { get; set; }
        public double flowtemp6 { get; set; }
        public double flowtemp7 { get; set; }
        public double flowtemp8 { get; set; }
        public double flowtemp9 { get; set; }
        public double flowtemp10 { get; set; }
        public double flowtemp11 { get; set; }
        public double flowtemp12 { get; set; }
        public double flowpulses1 { get; set; }
        public double flowpulses2 { get; set; }
        public double flowpulses3 { get; set; }
        public double flowpulses4 { get; set; }
        public double flowpulses5 { get; set; }
        public double flowpulses6 { get; set; }
        public double flowpulses7 { get; set; }
        public double flowpulses8 { get; set; }
        public double flowpulses9 { get; set; }
        public double flowpulses10 { get; set; }
        public double flowpulses11 { get; set; }
        public double flowpulses12 { get; set; }
        public double density1 { get; set; }
        public double density2 { get; set; }
        public double density3 { get; set; }
        public double density4 { get; set; }
        public double density5 { get; set; }
        public double density6 { get; set; }
        public double densitytemp1 { get; set; }
        public double densitytemp2 { get; set; }
        public double densitytemp3 { get; set; }
        public double densitytemp4 { get; set; }
        public double densitytemp5 { get; set; }
        public double densitytemp6 { get; set; }
        public double density1visco { get; set; }
        public double density2visco { get; set; }
        public double density3visco { get; set; }
        public double density4visco { get; set; }
        public double density5visco { get; set; }
        public double density6visco { get; set; }
        public double gps_sog { get; set; }
        public double gps_time { get; set; }
        public double gps_latitude { get; set; }
        public double gps_longitude { get; set; }
        public double speedlog_stw_longitudinal { get; set; }
        public double speedlog_stw_transversal { get; set; }
        public double speedlog_sog_longitudinal { get; set; }
        public double speedlog_sog_transversal { get; set; }
        public double heading { get; set; }
        public double windspeed { get; set; }
        public double windangle { get; set; }
        public double depth { get; set; }
        public double draftfront { get; set; }
        public double draftmiddle1 { get; set; }
        public double draftmiddle2 { get; set; }
        public double draftback { get; set; }
        public double roll { get; set; }
        public double yaw { get; set; }
        public double pitch { get; set; }
        public double cargo { get; set; }
        public double torque1 { get; set; }
        public double torque2 { get; set; }
        public double speed1 { get; set; }
        public double speed2 { get; set; }
        public double power1 { get; set; }
        public double power2 { get; set; }
        public double thrust1 { get; set; }
        public double thrust2 { get; set; }
        public double propellerpitch1 { get; set; }
        public double propellerpitch2 { get; set; }
        public double shaftgenerator1 { get; set; }
        public double shaftgenerator2 { get; set; }
        public double auxgenerator1 { get; set; }
        public double auxgenerator2 { get; set; }
        public double auxgenerator3 { get; set; }
        public double auxgenerator4 { get; set; }
        public double auxgenerator5 { get; set; }
        public double auxgenerator6 { get; set; }
        public double rudder1 { get; set; }
        public double rudder2 { get; set; }
        public double bearingorg { get; set; }
        public double bearing { get; set; }
        public double watertemp { get; set; }
        public double trackmade { get; set; }
        public double rateofturn { get; set; }
        public double shaftspeed1nmea { get; set; }
        public double shaftspeed2nmea { get; set; }
        public double waypointid { get; set; }
        public string waypointname { get; set; }
        public double distancewp { get; set; }
        public double destwplatitude { get; set; }
        public double destwplongitude { get; set; }
        public double wplatitude { get; set; }
        public double wplongitude { get; set; }

        public override void Randomize(int amountOfExistingModels, Random randomGenerator)
        {
            this.MinuteAveragesRowId = amountOfExistingModels + 1;
            this.waypointname = "SomeWaypoint";

            // Randomizing all the int64 fields via reflection cuz ain't no way I'm gonna write all that AGAIN
            var decimalProps = this.GetType()
                                  .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                  .Where(f => f.PropertyType == typeof(double))
                                  .ToArray();

            foreach (var decimalProp in decimalProps)
            {
                decimalProp.SetValue(this, randomGenerator.NextDouble());
            }
        }
    }

    /// PERST DB Can only work with models using public fields, whilst Entity Framework
    /// can only work with models using public PROPERTIES. In order to be still able to use both,
    /// the class below is an exact mirror of the one above, except it uses FIELDS instead of PROPERTIES
    /// so that PERST can still use it. This is not very 'clean', I know, I know...

    public class MinuteAveragesRowForPerst : AbstractModel<MinuteAveragesRowForPerst>
    {
        [IsPrimaryKey]
        public long MinuteAveragesRowId ;

        public double startid ;
        public double endid ;
        public double engine1 ;
        public double engine2 ;
        public double engine3 ;
        public double engine4 ;
        public double engine5 ;
        public double engine6 ;
        public double engine7 ;
        public double engine8 ;
        public double flow1 ;
        public double flow2 ;
        public double flow3 ;
        public double flow4 ;
        public double flow5 ;
        public double flow6 ;
        public double flow7 ;
        public double flow8 ;
        public double flow9 ;
        public double flow10 ;
        public double flow11 ;
        public double flow12 ;
        public double flowtemp1 ;
        public double flowtemp2 ;
        public double flowtemp3 ;
        public double flowtemp4 ;
        public double flowtemp5 ;
        public double flowtemp6 ;
        public double flowtemp7 ;
        public double flowtemp8 ;
        public double flowtemp9 ;
        public double flowtemp10 ;
        public double flowtemp11 ;
        public double flowtemp12 ;
        public double flowpulses1 ;
        public double flowpulses2 ;
        public double flowpulses3 ;
        public double flowpulses4 ;
        public double flowpulses5 ;
        public double flowpulses6 ;
        public double flowpulses7 ;
        public double flowpulses8 ;
        public double flowpulses9 ;
        public double flowpulses10 ;
        public double flowpulses11 ;
        public double flowpulses12 ;
        public double density1 ;
        public double density2 ;
        public double density3 ;
        public double density4 ;
        public double density5 ;
        public double density6 ;
        public double densitytemp1 ;
        public double densitytemp2 ;
        public double densitytemp3 ;
        public double densitytemp4 ;
        public double densitytemp5 ;
        public double densitytemp6 ;
        public double density1visco ;
        public double density2visco ;
        public double density3visco ;
        public double density4visco ;
        public double density5visco ;
        public double density6visco ;
        public double gps_sog ;
        public double gps_time ;
        public double gps_latitude ;
        public double gps_longitude ;
        public double speedlog_stw_longitudinal ;
        public double speedlog_stw_transversal ;
        public double speedlog_sog_longitudinal ;
        public double speedlog_sog_transversal ;
        public double heading ;
        public double windspeed ;
        public double windangle ;
        public double depth ;
        public double draftfront ;
        public double draftmiddle1 ;
        public double draftmiddle2 ;
        public double draftback ;
        public double roll ;
        public double yaw ;
        public double pitch ;
        public double cargo ;
        public double torque1 ;
        public double torque2 ;
        public double speed1 ;
        public double speed2 ;
        public double power1 ;
        public double power2 ;
        public double thrust1 ;
        public double thrust2 ;
        public double propellerpitch1 ;
        public double propellerpitch2 ;
        public double shaftgenerator1 ;
        public double shaftgenerator2 ;
        public double auxgenerator1 ;
        public double auxgenerator2 ;
        public double auxgenerator3 ;
        public double auxgenerator4 ;
        public double auxgenerator5 ;
        public double auxgenerator6 ;
        public double rudder1 ;
        public double rudder2 ;
        public double bearingorg ;
        public double bearing ;
        public double watertemp ;
        public double trackmade ;
        public double rateofturn ;
        public double shaftspeed1nmea ;
        public double shaftspeed2nmea ;
        public double waypointid ;
        public string waypointname ;
        public double distancewp ;
        public double destwplatitude ;
        public double destwplongitude ;
        public double wplatitude ;
        public double wplongitude ;

        public override void Randomize(int amountOfExistingModels, Random randomGenerator)
        {
            this.MinuteAveragesRowId = amountOfExistingModels + 1;
            this.waypointname = "SomeWaypoint";

            // Randomizing all the int64 fields via reflection cuz ain't no way I'm gonna write all that AGAIN
            var decimalFields = this.GetType()
                                  .GetFields(BindingFlags.Instance | BindingFlags.Public)
                                  .Where(f => f.FieldType == typeof(double))
                                  .ToArray();

            foreach (var decimalField in decimalFields)
            {
                decimalField.SetValue(this, randomGenerator.NextDouble());
            }
        }
    }
}
