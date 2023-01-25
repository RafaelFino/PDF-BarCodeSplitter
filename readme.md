# PDF - BarCodeSplitter

## Rules
This software needs to split every page on input Invoice files by types, and each must be delivered on diferent folders:
- Thermal
- Paper

### FedEX
Pages with barcodes type PDF_417 are Thermal types

### UPS
Pages with text "invoice" are paper type (case insensitive)

## Nuget Deps
- [Magick.Net](https://github.com/dlemstra/Magick.NET): To Tranform PDF into PNG files (Depends on GhostScript)
- [ZXing.net](https://www.nuget.org/packages/ZXing.Net/): To Recognize barcodes on PNG files
- [PDFSharp](http://www.pdfsharp.net/NuGetPackage_PDFsharp-MigraDoc-gdi.ashx): To split PDF
- [PDFPig](https://uglytoad.github.io/PdfPig/): To extract text from PDF files

## Lib Deps
- [GS - GhostScript](https://www.ghostscript.com/): To render PDF to Magick.Net
