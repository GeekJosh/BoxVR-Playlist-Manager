SET BOXVR=%1
SET BOXVR=%BOXVR:"=%

SET BOXVR_DATA=%2
SET BOXVR_DATA=%BOXVR_DATA:"=%

SET S_PATH=%3
SET S_PATH=%S_PATH:"=%

SET S_NAME=%4
SET S_NAME=%S_NAME:"=%

SET S_EXT=%5
SET S_EXT=%S_EXT:"=%

REM Step 1: Convert to WAV
IF NOT EXIST "%BOXVR_DATA%\%S_NAME%.wav" "%BOXVR%\BOXVR_Data\StreamingAssets\ffmpeg\bin\ffmpeg.exe" -y -i "%S_PATH%\%S_NAME%%S_EXT%" -ar 44100 "%BOXVR_DATA%\%S_NAME%.wav"

REM Step 2: Track beats
IF NOT EXIST "%BOXVR_DATA%\%S_NAME%.madmom.txt" "%BOXVR%\BOXVR_Data\StreamingAssets\DBNDownBeatTracker\DBNDownBeatTracker.exe" --beats_per_bar 4 single "%BOXVR_DATA%\%S_NAME%.wav" -o "%BOXVR_DATA%\%S_NAME%.madmom.txt" -j 1