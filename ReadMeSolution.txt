
Front: http://localhost:65461/
Back: http://localhost:49158/
https: https://localhost:44328/

====================================
This solution is NOT "best practices", look in Quicksilver for that.
This is just a site for doing course exercises and demos.

The Episerver-Find part is using my personal index... change to your dev-index

I've cleaned up a lot, but there is still some garbage

Unzip to c:\Episerver612S\ ... and it should start

====================================
13:48 2018-12-07
 - DB cleaned
====================================
2018-12-07
 - New solution site works (c:\Episerver\612S)
 - BU to .zip
 - NuGet (latest for ECF - 12.13)
	PromoContentLoader has changed (internal NS) - build error :=)
 - Site starts
	Migration Steps
 - CheckOut works
 - ComMan starts
...looks good so far
====================================
Need attention
 - Added code in accesory-view (no controller), variation controller and TaxCalc... and a luxury-tax för London
	plus stuff in VariationController and the "default-view" 
 ====================================
 Changed
  - Default market & WH (props)
	...to fit the tax-stuff/WH-stuff
====================================
Luxury
VAT
Tax-debug:
 - Variation controller
	UK
	sv taxValues Homeland 25% - originalTax 0 - originalTax2 0 - Money tax 0,01 - _taxCalculator.GetSalesTax 0,01
 - TaxCalc
	UK tm used to get 2 rows 50 + 99 (SomeTax) - TaxValues same 2 rows - originalTax3 149 (all the luxury-thing)
	sv
 - view
	UK t.GetSalesTax 149 (luxury-thing) ... when having 2 rows
	sv
====================================
BootStrap

====================================
admin/store



