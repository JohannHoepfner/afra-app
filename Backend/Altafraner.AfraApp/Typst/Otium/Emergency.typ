#let (
    titel,
    zeitpunkt,
    block,
    fehlende,
    termine,
) = json(bytes(sys.inputs.data))

#let text-color = rgb("#000")
#let gray-color = rgb("#aaa")
#let accent-color = rgb("#0069B4")
#let accent-font = ("TheSerif", "TimesNewRoman", "Noto Serif", "Noto Sans", "Arial")
#let primary-font = ("TheSansOSF", "Noto Sans", "Arial")

#show heading.where(level: 1): set text(size: 22pt, font: accent-font, weight: 500, fill: accent-color)
#show heading.where(level: 2): set text(size: 17pt, font: accent-font, weight: 500, fill: accent-color)
#show heading.where(level: 3): set text(size: 14pt, font: accent-font, weight: 500, fill: accent-color)

#set page(
  paper: "a4",
  margin: (
    left: 15mm,
    top: 10mm,
    right: 10mm,
    bottom: 10mm
  ),
  footer: align(left)[
    #text(size: 7pt, fill: gray-color)[
      Generiert in der Afra-App am #zeitpunkt
    ]
  ]
)
#set text(size: 12pt, fill: text-color, font: primary-font)

= #titel

Block: *#block*

== Fehlende

#if fehlende.len() == 0 [
  _Keine fehlenden Personen._
] else [
  #table(
    columns: (1fr, auto),
    inset: 6pt,
    stroke: 0.5pt + gray-color,
    table.header(
      [*Name*], [*Status*],
    ),
    ..for p in fehlende {
      ([#p.nachname, #p.vorname], [#p.status])
    }
  )
]

== Termine

#for t in termine [
  === #t.ort -- #t.bezeichnung

  #if t.einschreibungen.len() == 0 [
    _Keine Einschreibungen._
  ] else [
    #table(
      columns: (1fr, auto),
      inset: 6pt,
      stroke: 0.5pt + gray-color,
      table.header(
        [*Name*], [*Status*],
      ),
      ..for e in t.einschreibungen {
        ([#e.nachname, #e.vorname], [#e.status])
      }
    )
  ]
]
