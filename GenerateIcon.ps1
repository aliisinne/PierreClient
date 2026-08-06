Add-Type -AssemblyName System.Drawing

$size = 256
$bmp = New-Object System.Drawing.Bitmap $size, $size
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias

$greenColor = [System.Drawing.ColorTranslator]::FromHtml("#00E676")
$g.Clear([System.Drawing.Color]::Transparent)

$cornerRadius = 40
$path = New-Object System.Drawing.Drawing2D.GraphicsPath
$path.AddArc(0, 0, $cornerRadius, $cornerRadius, 180, 90)
$path.AddArc($size - $cornerRadius, 0, $cornerRadius, $cornerRadius, 270, 90)
$path.AddArc($size - $cornerRadius, $size - $cornerRadius, $cornerRadius, $cornerRadius, 0, 90)
$path.AddArc(0, $size - $cornerRadius, $cornerRadius, $cornerRadius, 90, 90)
$path.CloseFigure()

$brush = New-Object System.Drawing.SolidBrush $greenColor
$g.FillPath($brush, $path)
$brush.Dispose()
$path.Dispose()

$font = New-Object System.Drawing.Font("Segoe UI", 140, [System.Drawing.FontStyle]::Bold)
$textSize = $g.MeasureString("P", $font)
$x = ($size - $textSize.Width) / 2
$y = ($size - $textSize.Height) / 2
$textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$g.DrawString("P", $font, $textBrush, $x + 5, $y)

$textBrush.Dispose()
$font.Dispose()
$g.Dispose()

$bmp.Save("C:\Users\mymai\OneDrive\Belgeler\PierreClient1.21.11\PierreLauncher\Assets\icon.png", [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()

Write-Host "Icon created successfully!"
