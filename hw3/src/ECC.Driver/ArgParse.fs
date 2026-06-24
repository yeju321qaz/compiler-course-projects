(*
  ECC - Ewha C-- Compiler

  Copyright (c) SWEETS Lab. @ Ewha Womans University, since 2026

  Permission is hereby granted, free of charge, to any person obtaining a copy of
  this software and associated documentation files (the "Software"), to deal in
  the Software without restriction, including without limitation the rights to
  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
  of the Software, and to permit persons to whom the Software is furnished to do
  so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all
  copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
  SOFTWARE.
*)

module ECC.Driver.ArgParse

open Argu
open System.IO
open System.Text

open ECC.Core

let createParser () =
    ArgumentParser.Create<CmdArgs> (programName = "ECC.Driver")

let validateArgs (args: ParseResults<CmdArgs>) =
    let inputPath = args.GetResult Input
    if args.Contains Version then
        printfn "ECC (Ewha C-- Compiler) ver. %s" Version.versionStr
        ImmediateExit
    elif File.Exists inputPath |> not then 
        ArgParseFailure <| sprintf "Input file does not exist: %s" inputPath
    else
        let runMode = args.GetResult (Mode, defaultValue = RunAll)
        ArgParseSuccess (runMode, inputPath)

let handleError (ex: exn) =
    let builder = StringBuilder ()
    StringUtils.appendString builder <| sprintf "Error: %s" ex.Message
    ArgParseFailure <| builder.ToString ()

let parseArgs args =
    let parser = createParser ()
    try
        let args = parser.Parse args
        validateArgs args
    with
        | :? ArguParseException as ex ->
            handleError ex
