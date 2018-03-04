﻿using AlphaTab.Audio.Synth.Midi;
using AlphaTab.Audio.Synth.Midi.Event;
using AlphaTab.Model;

namespace AlphaTab.Audio.Generator
{
    public class AlphaSynthMidiFileHandler : IMidiFileHandler
    {
        private readonly MidiFile _midiFile;

        public AlphaSynthMidiFileHandler(MidiFile midiFile)
        {
            _midiFile = midiFile;
        }

        public void AddTimeSignature(int tick, int timeSignatureNumerator, int timeSignatureDenominator)
        {
            var denominatorIndex = 0;
            while ((timeSignatureDenominator = (timeSignatureDenominator >> 1)) > 0)
            {
                denominatorIndex++;
            }

            var message = new MetaDataEvent(tick, 
                0xFF, 
                (byte)MetaEventTypeEnum.TimeSignature, 
                new byte[] { (byte)(timeSignatureNumerator & 0xFF), (byte)(denominatorIndex & 0xFF), 48, 8 });

            _midiFile.Events.Add(message);
        }

        public void AddRest(int track, int tick, int channel)
        {
            var message = new SystemExclusiveEvent(tick, (byte) SystemCommonTypeEnum.SystemExclusive, 0, new byte[] {0xFF});
            _midiFile.Events.Add(message);
        }

        public void AddNote(int track, int start, int length, byte key, DynamicValue dynamicValue, byte channel)
        {
            var velocity = MidiUtils.DynamicToVelocity(dynamicValue);

            var noteOn = new MidiEvent(start, MakeCommand((byte)MidiEventTypeEnum.NoteOn, channel), FixValue(key), FixValue((byte)velocity));
            _midiFile.Events.Add(noteOn);

            var noteOff = new MidiEvent(start + length, MakeCommand((byte)MidiEventTypeEnum.NoteOff, channel), FixValue(key), FixValue((byte)velocity));
            _midiFile.Events.Add(noteOff);
        }

        private byte MakeCommand(byte command, byte channel)
        {
            return (byte)((command & 0xF0) | (channel & 0x0F));
        }

        private static byte FixValue(byte value)
        {
            if (value > 127) return 127;
            return value;
        }

        public void AddControlChange(int track, int tick, byte channel, byte controller, byte value)
        {
            var message = new MidiEvent(tick, MakeCommand((byte)MidiEventTypeEnum.Controller, channel), FixValue(controller), FixValue(value));
            _midiFile.Events.Add(message);
        }

        public void AddProgramChange(int track, int tick, byte channel, byte program)
        {
            var message = new MidiEvent(tick, MakeCommand((byte)MidiEventTypeEnum.ProgramChange, channel), FixValue(program), 0);
            _midiFile.Events.Add(message);
        }

        public void AddTempo(int tick, int tempo)
        {
            // bpm -> microsecond per quarter note
            var tempoInUsq = (60000000 / tempo);

            var message = new MetaNumberEvent(tick,
                0xFF,
                (byte)MetaEventTypeEnum.Tempo,
                tempoInUsq);
            _midiFile.Events.Add(message);
        }

        public void AddBend(int track, int tick, byte channel, byte value)
        {
            var message = new MidiEvent(tick, MakeCommand((byte)MidiEventTypeEnum.PitchBend, channel), 0, FixValue(value));
            _midiFile.Events.Add(message);
        }

        public void FinishTrack(int track, int tick)
        {
            var message = new MetaDataEvent(tick,
                0xFF,
                (byte)MetaEventTypeEnum.EndOfTrack,
                new byte[0]);
            _midiFile.Events.Add(message);
        }
    }
}
