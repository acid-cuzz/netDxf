# Downcomer retrofit insert drawings (DWG-DCINS-01..03, Rev B)

This console project uses netDxf to redraw the *Downcomer Retrofit Insert, Concept Rev B* drawing set as
DXF files. The drawings are built from the part dimensions, so if a site dimension changes you change it in
one place and regenerate.

| File | Sheet | Content |
|---|---|---|
| `Drawings/DWG-DCINS-01.dxf` | 1 of 3 | Section A-A assembled (2:1), detail C standpipe seal (4:1), plan on trough floor (1:1), parts list, legend, design intent |
| `Drawings/DWG-DCINS-02.dxf` | 2 of 3 | Item 1 insert (2:1), item 5 seal retainer (3:1), item 2 nut (2:1), feature table, seal gland data, lathe operations, manufacturing notes |
| `Drawings/DWG-DCINS-03.dxf` | 3 of 3 | Installation sequence sketches a–e (1:1.5), which repair, design basis, installation, fit check, inspection, limits of use, to be confirmed |
| `Drawings/DWG-DCINS-01-03_Rev_B_preview.pdf` | all | Preview of the three DXFs, rendered with ezdxf (for viewing without CAD) |

## Regenerating

```
dotnet run --project DowncomerRetrofit -c Release -- DowncomerRetrofit/Drawings
```

The program writes the three DXFs to the folder given (default `Drawings`), then reads each one back with
netDxf as a check.

## How the drawings are set up

* AutoCAD 2013 DXF, millimetres. Each sheet is an A3 landscape sheet (420 × 297) drawn 1:1 in model space,
  so it plots at 1:1 on A3. Every view is drawn at its own scale. Each dimensioned scale has its own
  dimension style (`DCINS-2TO1`, `DCINS-3TO1`, `DCINS-1TO1`, `DCINS-1TO1.5`), whose linear factor makes
  the dimensions read in true part size.
* Layers: `VISIBLE`, `THIN`, `HIDDEN`, `CENTER`, `HATCH`, `FILL`, `DIMENSIONS`, `NOTES`, `TABLES`, `BORDER`.
  Lineweights are set on the layers; the coloured section fills are on `FILL` and can be frozen for a
  black-and-white print.
* Text uses the `DCINS` / `DCINS-BOLD` styles on `arial.ttf` / `arialbd.ttf`.
* Section hatching is stored in the file as user-defined (single lines) or a named custom pattern (PFA
  cross-hatch), so it does not depend on `acad.pat` or `acadiso.pat`.

## Where things are

* `Parts.cs`: `Spec` holds every dimension (site values are the assumed ones from the drawing notes).
  `Parts` turns them into half-section profiles in (r, z), with z = 0 at the seat (flange underside).
* `Assembly.cs`: the sectioned assembly of one position. It is used by section A-A, by detail C (clipped to
  the detail circle) and by the five installation sketches.
* `Sheet.cs`: the sheet framework: layers, materials, view scaling and clipping, text, leaders, balloons,
  dimensions, tables and the title block.
* `Sheet1.cs`, `Sheet2.cs`, `Sheet3.cs`: one file per sheet.

## Differences from the PDF

The content (dimensions, notes, tables and title block) is the same as the PDF. These drafting details changed:

* Adjacent parts in section are hatched at different angles or spacings (in the PDF the insert and both nuts
  were hatched the same).
* The plan now shows the A-A cutting plane, and each position has centre marks.
* The item 2 detail shows the internal thread in the usual way: hatched to the minor diameter, with a thin
  line at the major diameter.
* The counterbore mouth chamfer is drawn at 0.5 × 30° as the notes say. The PDF drew it at 45°.
* The "PLAIN COUNTERBORE" leader in detail C now ends on the counterbore wall. The "10.5 CLEAR" note sits
  below the neighbouring tube so it no longer runs into it. General note 2 on sheet 1 is re-wrapped so it
  stays inside its box.
