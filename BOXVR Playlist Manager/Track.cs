using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoxVR_Playlist_Manager
{
    public class Track : EqualityComparer<Track>
    {
        const string STR_UNKNOWN = "<Unknown>";

        private ATL.Track _atlTrack;

        public string Path { get; private set; }

        public string Title => string.IsNullOrWhiteSpace(_atlTrack.Title) ? STR_UNKNOWN : _atlTrack.Title;

        public string Artist
        {
            get
            {
                var artist = _atlTrack.AlbumArtist;
                if (string.IsNullOrWhiteSpace(artist)) artist = _atlTrack.Artist;
                return string.IsNullOrWhiteSpace(artist) ? STR_UNKNOWN : artist;
            }
        }

        public string Album => string.IsNullOrWhiteSpace(_atlTrack.Album) ? STR_UNKNOWN : _atlTrack.Album;

        public int DurationInt => _atlTrack.Duration;
        public TimeSpan Duration => new TimeSpan(0, 0, DurationInt);

        public Track(string path)
        {
            Path = path;
            _atlTrack = new ATL.Track(path);
        }

        public override bool Equals(Track x, Track y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is null || y is null)
                return false;

            return x.Path.Equals(y.Path, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode(Track obj) => base.GetHashCode();

        private static Dictionary<string, string> _supportedTrackFormats;
        public static Dictionary<string, string> SupportedTrackFormats
        {
            get
            {
                // this list pulled from ffmpeg version bundled with BoxVR on 15/11/18
                if (_supportedTrackFormats == null || !_supportedTrackFormats.Any())
                {
                    _supportedTrackFormats = new Dictionary<string, string>()
                    {
                        { "4X Technologies", "*.4xm" },
                        { "Audible AA format files", "*.aa" },
                        { "raw ADTS AAC (Advanced Audio Coding)", "*.aac" },
                        { "raw AC-3", "*.ac3" },
                        { "Interplay ACM", "*.acm" },
                        { "ACT Voice file format", "*.act" },
                        { "Artworx Data Format", "*.adf" },
                        { "ADP", "*.adp" },
                        { "Sony PS2 ADS", "*.ads" },
                        { "CRI ADX", "*.adx" },
                        { "MD STUDIO audio", "*.aea" },
                        { "AFC", "*.afc" },
                        { "Audio IFF", "*.aiff" },
                        { "CRI AIX", "*.aix" },
                        { "PCM A-law", "*.alaw" },
                        { "Alias/Wavefront PIX image", "*.alias_pix" },
                        { "3GPP AMR", "*.amr" },
                        { "raw AMR-NB", "*.amrnb" },
                        { "raw AMR-WB", "*.amrwb" },
                        { "Deluxe Paint Animation", "*.anm" },
                        { "CRYO APC", "*.apc" },
                        { "Monkey's Audio", "*.ape" },
                        { "Animated Portable Network Graphics", "*.apng" },
                        { "raw aptX (Audio Processing Technology for Bluetooth)", "*.aptx" },
                        { "raw aptX HD (Audio Processing Technology for Bluetooth)", "*.aptx_hd" },
                        { "AQTitle subtitles", "*.aqtitle" },
                        { "ASF (Advanced / Active Streaming Format)", "*.asf;*.asf_o" },
                        { "SSA (SubStation Alpha) subtitle", "*.ass" },
                        { "AST (Audio Stream)", "*.ast" },
                        { "Sun AU", "*.au" },
                        { "AVI (Audio Video Interleaved)", "*.avi" },
                        { "AviSynth script", "*.avisynth" },
                        { "AVR (Audio Visual Research)", "*.avr" },
                        { "AVS", "*.avs" },
                        { "Bethesda Softworks VID", "*.bethsoftvid" },
                        { "Brute Force & Ignorance", "*.bfi" },
                        { "BFSTM (Binary Cafe Stream)", "*.bfstm" },
                        { "Binary text", "*.bin" },
                        { "Bink", "*.bink" },
                        { "G.729 BIT file format", "*.bit" },
                        { "piped bmp sequence", "*.bmp_pipe" },
                        { "Discworld II BMV", "*.bmv" },
                        { "Black Ops Audio", "*.boa" },
                        { "BRender PIX image", "*.brender_pix" },
                        { "BRSTM (Binary Revolution Stream)", "*.brstm" },
                        { "Interplay C93", "*.c93" },
                        { "Apple CAF (Core Audio Format)", "*.caf" },
                        { "raw Chinese AVS (Audio Video Standard) video", "*.cavsvideo" },
                        { "CD Graphics", "*.cdg" },
                        { "Commodore CDXL video", "*.cdxl" },
                        { "Phantom Cine", "*.cine" },
                        { "codec2 .c2 muxer", "*.codec2" },
                        { "raw codec2 muxer", "*.codec2raw" },
                        { "Virtual concatenation script", "*.concat" },
                        { "DASH Muxer", "*.dash" },
                        { "raw data", "*.data" },
                        { "D-Cinema audio", "*.daud" },
                        { "Sega DC STR", "*.dcstr" },
                        { "piped dds sequence", "*.dds_pipe" },
                        { "Chronomaster DFA", "*.dfa" },
                        { "raw Dirac", "*.dirac" },
                        { "raw DNxHD (SMPTE VC-3)", "*.dnxhd" },
                        { "piped dpx sequence", "*.dpx_pipe" },
                        { "DSD Stream File (DSF)", "*.dsf" },
                        { "DirectShow capture", "*.dshow" },
                        { "Delphine Software International CIN", "*.dsicin" },
                        { "Digital Speech Standard (DSS)", "*.dss" },
                        { "raw DTS", "*.dts" },
                        { "raw DTS-HD", "*.dtshd" },
                        { "DV (Digital Video)", "*.dv" },
                        { "raw dvbsub", "*.dvbsub" },
                        { "dvbtxt", "*.dvbtxt" },
                        { "DXA", "*.dxa" },
                        { "Electronic Arts Multimedia", "*.ea" },
                        { "Electronic Arts cdata", "*.ea_cdata" },
                        { "raw E-AC-3", "*.eac3" },
                        { "Ensoniq Paris Audio File", "*.epaf" },
                        { "piped exr sequence", "*.exr_pipe" },
                        { "PCM 32-bit floating-point big-endian", "*.f32be" },
                        { "PCM 32-bit floating-point little-endian", "*.f32le" },
                        { "PCM 64-bit floating-point big-endian", "*.f64be" },
                        { "PCM 64-bit floating-point little-endian", "*.f64le" },
                        { "FFmpeg metadata in text", "*.ffmetadata" },
                        { "Sega FILM / CPK", "*.film_cpk" },
                        { "Adobe Filmstrip", "*.filmstrip" },
                        { "Flexible Image Transport System", "*.fits" },
                        { "raw FLAC", "*.flac" },
                        { "FLI/FLC/FLX animation", "*.flic" },
                        { "FLV (Flash Video)", "*.flv" },
                        { "Megalux Frame", "*.frm" },
                        { "FMOD Sample Bank", "*.fsb" },
                        { "raw G.722", "*.g722" },
                        { "raw G.723.1", "*.g723_1" },
                        { "raw big-endian G.726 (\"left-justified\")", "*.g726" },
                        { "raw little-endian G.726 (\"right-justified\")", "*.g726le" },
                        { "G.729 raw format demuxer", "*.g729" },
                        { "GDI API Windows frame grabber", "*.gdigrab" },
                        { "Gremlin Digital Video", "*.gdv" },
                        { "GENeric Header", "*.genh" },
                        { "GIF Animation", "*.gif" },
                        { "raw GSM", "*.gsm" },
                        { "GXF (General eXchange Format)", "*.gxf" },
                        { "raw H.261", "*.h261" },
                        { "raw H.263", "*.h263" },
                        { "raw H.264 video", "*.h264" },
                        { "raw HEVC video", "*.hevc" },
                        { "Apple HTTP Live Streaming", "*.hls;*.applehttp" },
                        { "Cryo HNM v4", "*.hnm" },
                        { "Microsoft Windows ICO", "*.ico" },
                        { "id Cinematic", "*.idcin" },
                        { "iCE Draw File", "*.idf" },
                        { "IFF (Interchange File Format)", "*.iff" },
                        { "iLBC storage", "*.ilbc" },
                        { "image2 sequence", "*.image2" },
                        { "piped image2 sequence", "*.image2pipe" },
                        { "raw Ingenient MJPEG", "*.ingenient" },
                        { "Interplay MVE", "*.ipmovie" },
                        { "Berkeley/IRCAM/CARL Sound Format", "*.ircam" },
                        { "Funcom ISS", "*.iss" },
                        { "IndigoVision 8000 video", "*.iv8" },
                        { "On2 IVF", "*.ivf" },
                        { "IVR (Internet Video Recording)", "*.ivr" },
                        { "piped j2k sequence", "*.j2k_pipe" },
                        { "JACOsub subtitle format", "*.jacosub" },
                        { "piped jpeg sequence", "*.jpeg_pipe" },
                        { "piped jpegls sequence", "*.jpegls_pipe" },
                        { "Bitmap Brothers JV", "*.jv" },
                        { "Libavfilter virtual input device", "*.lavfi" },
                        { "live RTMP FLV (Flash Video)", "*.live_flv" },
                        { "raw lmlm4", "*.lmlm4" },
                        { "LOAS AudioSyncStream", "*.loas" },
                        { "LRC lyrics", "*.lrc" },
                        { "LVF", "*.lvf" },
                        { "VR native stream (LXF)", "*.lxf" },
                        { "raw MPEG-4 video", "*.m4v" },
                        { "Matroska / WebM", "*.matroska;*.webm" },
                        { "Metal Gear Solid: The Twin Snakes", "*.mgsts" },
                        { "MicroDVD subtitle format", "*.microdvd" },
                        { "raw MJPEG video", "*.mjpeg" },
                        { "raw MJPEG 2000 video", "*.mjpeg_2000" },
                        { "raw MLP", "*.mlp" },
                        { "Magic Lantern Video (MLV)", "*.mlv" },
                        { "American Laser Games MM", "*.mm" },
                        { "Yamaha SMAF", "*.mmf" },
                        { "QuickTime / MOV", "*.mov;*.mp4;*.m4a;*.3gp;*.3g2;*.mj2" },
                        { "MP3 (MPEG audio layer 3)", "*.mp3" },
                        { "Musepack", "*.mpc" },
                        { "Musepack SV8", "*.mpc8" },
                        { "MPEG-1 Systems / MPEG program stream", "*.mpeg" },
                        { "MPEG-TS (MPEG-2 Transport Stream)", "*.mpegts" },
                        { "raw MPEG-TS (MPEG-2 Transport Stream)", "*.mpegtsraw" },
                        { "raw MPEG video", "*.mpegvideo" },
                        { "MIME multipart JPEG", "*.mpjpeg" },
                        { "MPL2 subtitles", "*.mpl2" },
                        { "MPlayer subtitles", "*.mpsub" },
                        { "Sony PS3 MSF", "*.msf" },
                        { "MSN TCP Webcam stream", "*.msnwctcp" },
                        { "Konami PS2 MTAF", "*.mtaf" },
                        { "MTV", "*.mtv" },
                        { "PCM mu-law", "*.mulaw" },
                        { "Eurocom MUSX", "*.musx" },
                        { "Silicon Graphics Movie", "*.mv" },
                        { "Motion Pixels MVI", "*.mvi" },
                        { "MXF (Material eXchange Format)", "*.mxf" },
                        { "MxPEG clip", "*.mxg" },
                        { "NC camera feed", "*.nc" },
                        { "NIST SPeech HEader REsources", "*.nistsphere" },
                        { "Computerized Speech Lab NSP", "*.nsp" },
                        { "Nullsoft Streaming Video", "*.nsv" },
                        { "NUT", "*.nut" },
                        { "NuppelVideo", "*.nuv" },
                        { "Ogg", "*.ogg" },
                        { "Sony OpenMG audio", "*.oma" },
                        { "Amazing Studio Packed Animation File", "*.paf" },
                        { "piped pam sequence", "*.pam_pipe" },
                        { "piped pbm sequence", "*.pbm_pipe" },
                        { "piped pcx sequence", "*.pcx_pipe" },
                        { "piped pgm sequence", "*.pgm_pipe" },
                        { "piped pgmyuv sequence", "*.pgmyuv_pipe" },
                        { "piped pictor sequence", "*.pictor_pipe" },
                        { "PJS (Phoenix Japanimation Society) subtitles", "*.pjs" },
                        { "Playstation Portable PMP", "*.pmp" },
                        { "piped png sequence", "*.png_pipe" },
                        { "piped ppm sequence", "*.ppm_pipe" },
                        { "piped psd sequence", "*.psd_pipe" },
                        { "Sony Playstation STR", "*.psxstr" },
                        { "TechnoTrend PVA", "*.pva" },
                        { "PVF (Portable Voice Format)", "*.pvf" },
                        { "QCP", "*.qcp" },
                        { "piped qdraw sequence", "*.qdraw_pipe" },
                        { "REDCODE R3D", "*.r3d" },
                        { "raw video", "*.rawvideo" },
                        { "RealText subtitle format", "*.realtext" },
                        { "RedSpark", "*.redspark" },
                        { "RL2", "*.rl2" },
                        { "RealMedia", "*.rm" },
                        { "raw id RoQ", "*.roq" },
                        { "RPL / ARMovie", "*.rpl" },
                        { "GameCube RSD", "*.rsd" },
                        { "Lego Mindstorms RSO", "*.rso" },
                        { "RTP output", "*.rtp" },
                        { "RTSP output", "*.rtsp" },
                        { "PCM signed 16-bit big-endian", "*.s16be" },
                        { "PCM signed 16-bit little-endian", "*.s16le" },
                        { "PCM signed 24-bit big-endian", "*.s24be" },
                        { "PCM signed 24-bit little-endian", "*.s24le" },
                        { "PCM signed 32-bit big-endian", "*.s32be" },
                        { "PCM signed 32-bit little-endian", "*.s32le" },
                        { "SMPTE 337M", "*.s337m" },
                        { "PCM signed 8-bit", "*.s8" },
                        { "SAMI subtitle format", "*.sami" },
                        { "SAP output", "*.sap" },
                        { "raw SBC", "*.sbc" },
                        { "SBaGen binaural beats script", "*.sbg" },
                        { "Scenarist Closed Captions", "*.scc" },
                        { "SDP", "*.sdp" },
                        { "SDR2", "*.sdr2" },
                        { "MIDI Sample Dump Standard", "*.sds" },
                        { "Sample Dump eXchange", "*.sdx" },
                        { "piped sgi sequence", "*.sgi_pipe" },
                        { "raw Shorten", "*.shn" },
                        { "Beam Software SIFF", "*.siff" },
                        { "Asterisk raw pcm", "*.sln" },
                        { "Loki SDL MJPEG", "*.smjpeg" },
                        { "Smacker", "*.smk" },
                        { "LucasArts Smush", "*.smush" },
                        { "Sierra SOL", "*.sol" },
                        { "SoX native", "*.sox" },
                        { "IEC 61937 (used on S/PDIF - IEC958)", "*.spdif" },
                        { "SubRip subtitle", "*.srt" },
                        { "Spruce subtitle format", "*.stl" },
                        { "SubViewer subtitle format", "*.subviewer" },
                        { "SubViewer v1 subtitle format", "*.subviewer1" },
                        { "piped sunrast sequence", "*.sunrast_pipe" },
                        { "raw HDMV Presentation Graphic Stream subtitles", "*.sup" },
                        { "Konami PS2 SVAG", "*.svag" },
                        { "piped svg sequence", "*.svg_pipe" },
                        { "SWF (ShockWave Flash)", "*.swf" },
                        { "raw TAK", "*.tak" },
                        { "TED Talks captions", "*.tedcaptions" },
                        { "THP", "*.thp" },
                        { "Tiertex Limited SEQ", "*.tiertexseq" },
                        { "piped tiff sequence", "*.tiff_pipe" },
                        { "8088flex TMV", "*.tmv" },
                        { "raw TrueHD", "*.truehd" },
                        { "TTA (True Audio)", "*.tta" },
                        { "Tele-typewriter", "*.tty" },
                        { "Renderware TeXture Dictionary", "*.txd" },
                        { "TiVo TY Stream", "*.ty" },
                        { "PCM unsigned 16-bit big-endian", "*.u16be" },
                        { "PCM unsigned 16-bit little-endian", "*.u16le" },
                        { "PCM unsigned 24-bit big-endian", "*.u24be" },
                        { "PCM unsigned 24-bit little-endian", "*.u24le" },
                        { "PCM unsigned 32-bit big-endian", "*.u32be" },
                        { "PCM unsigned 32-bit little-endian", "*.u32le" },
                        { "PCM unsigned 8-bit", "*.u8" },
                        { "Uncompressed 4:2:2 10-bit", "*.v210;*.v201x" },
                        { "Sony PS2 VAG", "*.vag" },
                        { "raw VC-1 video", "*.vc1" },
                        { "VC-1 test bitstream", "*.vc1test" },
                        { "VfW video capture", "*.vfwcap" },
                        { "Vivo", "*.vivo" },
                        { "Sierra VMD", "*.vmd" },
                        { "VobSub subtitle format", "*.vobsub" },
                        { "Creative Voice", "*.voc" },
                        { "Sony PS2 VPK", "*.vpk" },
                        { "VPlayer subtitles", "*.vplayer" },
                        { "Nippon Telegraph and Telephone Corporation (NTT) TwinVQ", "*.vqf" },
                        { "Sony Wave64", "*.w64" },
                        { "WAV / WAVE (Waveform Audio)", "*.wav" },
                        { "Wing Commander III movie", "*.wc3movie" },
                        { "WebM DASH Manifest", "*.webm_dash_manifest" },
                        { "piped webp sequence", "*.webp_pipe" },
                        { "WebVTT subtitle", "*.webvtt" },
                        { "Westwood Studios audio", "*.wsaud" },
                        { "Wideband Single-bit Data (WSD)", "*.wsd" },
                        { "Westwood Studios VQA", "*.wsvqa" },
                        { "Windows Television (WTV)", "*.wtv" },
                        { "raw WavPack", "*.wv" },
                        { "Psion 3 audio", "*.wve" },
                        { "Maxis XA", "*.xa" },
                        { "eXtended BINary text (XBIN)", "*.xbin" },
                        { "Microsoft XMV", "*.xmv" },
                        { "piped xpm sequence", "*.xpm_pipe" },
                        { "Sony PS3 XVAG", "*.xvag" },
                        { "Microsoft xWMA", "*.xwma" },
                        { "Psygnosis YOP", "*.yop" },
                        { "YUV4MPEG pipe", "*.yuv4mpegpipe" },

                    };
                }

                return _supportedTrackFormats;
            }
        }

        private static string _fileDialogFilter;
        public static string FileDialogFilter
        {
            get
            {
                if (string.IsNullOrEmpty(_fileDialogFilter))
                {
                    var allFiletypes = new List<string>();
                    foreach (var format in SupportedTrackFormats)
                        allFiletypes.Add(format.Value);

                    var filter = new StringBuilder();
                    filter.Append($"All supported files|{string.Join(";", allFiletypes)}");

                    foreach (var format in SupportedTrackFormats)
                        filter.Append($"|{format.Key}|{format.Value}");

                    _fileDialogFilter = filter.ToString();
                }

                return _fileDialogFilter;
            }
        }
    }
}
