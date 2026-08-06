Add-Type -AssemblyName System.Drawing

$pngFile = "PierreLauncher\Assets\icon.png"
$icoFile = "PierreLauncher\Assets\icon.ico"

$bmp = [System.Drawing.Bitmap]::FromFile($pngFile)

$fs = New-Object System.IO.FileStream($icoFile, [System.IO.FileMode]::Create)
$bw = New-Object System.IO.BinaryWriter($fs)

# Write ICONDIR header
$bw.Write([int16]0) # Reserved
$bw.Write([int16]1) # Image type (1=ico)
$bw.Write([int16]1) # Image count

# Write ICONDIRENTRY
$width = if ($bmp.Width -ge 256) { 0 } else { $bmp.Width }
$height = if ($bmp.Height -ge 256) { 0 } else { $bmp.Height }

$bw.Write([byte]$width)
$bw.Write([byte]$height)
$bw.Write([byte]0) # Colors (0 = >= 8bpp)
$bw.Write([byte]0) # Reserved
$bw.Write([int16]0) # Color planes
$bw.Write([int16]32) # Bits per pixel

$ms = New-Object System.IO.MemoryStream
$bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
$ms.Position = 0
$pngBytes = $ms.ToArray()

$bw.Write([int32]$pngBytes.Length)
$bw.Write([int32]22) # Offset of image data (6 bytes header + 16 bytes entry)

# Write image data
$bw.Write($pngBytes)

$bw.Close()
$fs.Close()
$ms.Close()
$bmp.Dispose()

Write-Host "Converted to icon.ico successfully!"
