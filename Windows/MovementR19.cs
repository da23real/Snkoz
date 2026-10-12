using System;using System.Linq;using System.Drawing;using System.Windows.Forms;
namespace Synkoz {
 public sealed partial class Effect936 {public Binding15 MoveXR19=new Binding15{Min=-10,Max=10},MoveYR19=new Binding15{Min=-10,Max=10};public double MoveCenterXR19,MoveCenterYR19,MoveAmplitudeXR19=10,MoveAmplitudeYR19;}
 public sealed partial class EffectsForm936 {
 void MovementFieldsR19(Effect936 e){section15="Paramètres";Number("Centre X (%)",e.MoveCenterXR19,-1000,1000,v=>e.MoveCenterXR19=v);Number("Amplitude X (%)",e.MoveAmplitudeXR19,-1000,1000,v=>e.MoveAmplitudeXR19=v);MidiBindingFieldsR4("Mouvement X",e.MoveXR19);Number("Centre Y (%)",e.MoveCenterYR19,-1000,1000,v=>e.MoveCenterYR19=v);Number("Amplitude Y (%)",e.MoveAmplitudeYR19,-1000,1000,v=>e.MoveAmplitudeYR19=v);MidiBindingFieldsR4("Mouvement Y",e.MoveYR19);var length=Combo(Enum.GetValues(typeof(Length936)).Cast<object>().ToArray(),e.Length);length.SelectedIndexChanged+=delegate{e.Length=(Length936)length.SelectedItem;};Field("Durée du mouvement simple par note",length);Envelope(e);}
 }
 sealed partial class EffectRun936 {internal double MovementR19(Binding15 binding,double center,double amplitude,double time){return (center+(binding.MidiR4==null?amplitude*GraphEnvelope15:MidiBoundR4(binding,time,0)))*AttenuationR9;}}
}
