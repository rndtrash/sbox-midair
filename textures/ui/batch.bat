@echo off
FOR %%A IN (*.svg) DO inkscape %%A --export-type=png