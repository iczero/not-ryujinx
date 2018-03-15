using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ryujinx.Audio.OpenAL
{
    public class OpenALAudioOut : IAalOutput
    {
        private const int MaxTracks = 256;

        private AudioContext Context;

        private class Track : IDisposable
        {
            public int SourceId { get; private set; }

            public int SampleRate { get; private set; }
            
            public ALFormat Format { get; private set; }

            public PlaybackState State { get; set; }

            private ConcurrentDictionary<long, int> Buffers;

            private HashSet<long> QueuedTagsHash;
            private Queue<long>   QueuedTagsQueue;

            private bool Disposed;

            public Track(int SampleRate, ALFormat Format)
            {
                this.SampleRate = SampleRate;
                this.Format     = Format;

                State = PlaybackState.Stopped;

                SourceId = AL.GenSource();

                Buffers = new ConcurrentDictionary<long, int>();

                QueuedTagsHash  = new HashSet<long>();
                QueuedTagsQueue = new Queue<long>();
            }

            public int GetBufferId(long Tag)
            {
                if (Disposed)
                {
                    throw new ObjectDisposedException(nameof(Track));
                }

                int Id = AL.GenBuffer();

                Buffers.AddOrUpdate(Tag, Id, (Key, OldId) =>
                {
                    AL.DeleteBuffer(OldId);

                    return Id;
                });

                QueuedTagsHash.Add(Tag);
                QueuedTagsQueue.Enqueue(Tag);

                return Id;
            }

            public long[] GetReleasedBuffers()
            {
                SyncQueuedTags();

                ClearQueue();

                List<long> Tags = new List<long>();

                foreach (long Tag in Buffers.Keys)
                {
                    if (!QueuedTagsHash.Contains(Tag))
                    {
                        Tags.Add(Tag);
                    }
                }

                return Tags.ToArray();
            }

            public bool ContainsBuffer(long Tag)
            {
                SyncQueuedTags();

                return QueuedTagsHash.Contains(Tag);
            }

            public void ClearQueue()
            {
                AL.GetSource(SourceId, ALGetSourcei.BuffersProcessed, out int ReleasedCount);

                if (ReleasedCount > 0)
                {
                    AL.SourceUnqueueBuffers(SourceId, ReleasedCount);
                }
            }

            private void SyncQueuedTags()
            {
                AL.GetSource(SourceId, ALGetSourcei.BuffersQueued,    out int QueuedCount);
                AL.GetSource(SourceId, ALGetSourcei.BuffersProcessed, out int ReleasedCount);

                QueuedCount -= ReleasedCount;

                while (QueuedTagsQueue.Count > QueuedCount)
                {
                    QueuedTagsHash.Remove(QueuedTagsQueue.Dequeue());
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose(bool Disposing)
            {
                if (Disposing && !Disposed)
                {
                    Disposed = true;

                    AL.DeleteSource(SourceId);

                    foreach (int Id in Buffers.Values)
                    {
                        AL.DeleteBuffer(Id);
                    }
                }
            }
        }

        private ConcurrentDictionary<int, Track> Tracks;

        public OpenALAudioOut()
        {
            Context = new AudioContext();

            Tracks = new ConcurrentDictionary<int, Track>();
        }

        public int OpenTrack(int SampleRate, int Channels, out AudioFormat Format)
        {
            Format = AudioFormat.PcmInt16;

            Track Td = new Track(SampleRate, GetALFormat(Channels, Format));

            for (int Id = 0; Id < MaxTracks; Id++)
            {
                if (Tracks.TryAdd(Id, Td))
                {
                    return Id;
                }
            }

            return -1;
        }

        private ALFormat GetALFormat(int Channels, AudioFormat Format)
        {
            if (Channels < 1 || Channels > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(Channels));
            }

            if (Channels == 1)
            {
                switch (Format)
                {
                    case AudioFormat.PcmInt8:  return ALFormat.Mono8;
                    case AudioFormat.PcmInt16: return ALFormat.Mono16;
                }
            }
            else /* if (Channels == 2) */
            {
                switch (Format)
                {
                    case AudioFormat.PcmInt8:  return ALFormat.Stereo8;
                    case AudioFormat.PcmInt16: return ALFormat.Stereo16;
                }
            }
            
            throw new ArgumentException(nameof(Format));
        }

        public void CloseTrack(int Track)
        {
            if (Tracks.TryRemove(Track, out Track Td))
            {
                Td.Dispose();
            }
        }

        public void AppendBuffer(int Track, long Tag, byte[] Buffer)
        {
            if (Tracks.TryGetValue(Track, out Track Td))
            {
                int BufferId = Td.GetBufferId(Tag);

                AL.BufferData(BufferId, Td.Format, Buffer, Buffer.Length, Td.SampleRate);

                Td.ClearQueue();

                AL.SourceQueueBuffer(Td.SourceId, BufferId);

                StartPlaybackIfNeeded(Td);
            }
        }

        public bool ContainsBuffer(int Track, long Tag)
        {
            if (Tracks.TryGetValue(Track, out Track Td))
            {
                return Td.ContainsBuffer(Tag);
            }
            
            return false;
        }

        public long[] GetReleasedBuffers(int Track)
        {
            if (Tracks.TryGetValue(Track, out Track Td))
            {
                return Td.GetReleasedBuffers();
            }
            
            return null;
        }

        public void Start(int Track)
        {
            if (Tracks.TryGetValue(Track, out Track Td))
            {
                Td.State = PlaybackState.Playing;

                StartPlaybackIfNeeded(Td);
            }
        }

        private void StartPlaybackIfNeeded(Track Td)
        {
            AL.GetSource(Td.SourceId, ALGetSourcei.SourceState, out int StateInt);
                
            ALSourceState State = (ALSourceState)StateInt;

            if (State != ALSourceState.Playing && Td.State == PlaybackState.Playing)
            {
                Td.ClearQueue();

                AL.SourcePlay(Td.SourceId);
            }
        }        

        public void Stop(int Track)
        {
            if (Tracks.TryGetValue(Track, out Track Td))
            {
                Td.State = PlaybackState.Stopped;

                AL.SourceStop(Td.SourceId);
            }
        }

        public PlaybackState GetState(int Track)
        {
            if (Tracks.TryGetValue(Track, out Track Td))
            {
                return Td.State;
            }

            return PlaybackState.Stopped;
        }
    }
}