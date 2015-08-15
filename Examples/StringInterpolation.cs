﻿using static Pihrtsoft.Text.RegularExpressions.Linq.Patterns;

namespace Pihrtsoft.Text.RegularExpressions.Linq
{
    public static class StringInterpolation
    {
        public static Pattern Pattern => 
            "$\""
                + Open()
                + MaybeMany(
                    OneMany(NamedGroup("open", "$\"") + Open())
                    + OneMany(BalancingGroup("close", "open", '"') + Close())
                )
                + '"';

        private static Pattern Open()
        {
            return
                TextPart()
                + MaybeMany('{' + CodePart() + '}' + TextPart())
                + Any(
                    Assert('"'),
                    '{' + CodePart() + Assert("$\"")
                );
        }

        private static Pattern Close()
        {
            return
                CodePart()
                + MaybeMany('}' + TextPart() + '{' + CodePart())
                + Any(
                    Assert("$\""),
                    '}' + TextPart() + Assert('"')
                );
        }

        private static Pattern TextPart()
        {
            var whileNot = WhileNotChar('\\', '{', '"');

            return NamedGroup("text",
                whileNot
                + MaybeMany(
                    Any(
                        '\\' + NotNewLineChar(),
                        "{{"
                    )
                    + whileNot
                )
            );
        }

        private static Pattern CodePart()
        {
            var whileNot = WhileNotChar('\'', '"', '@', '/', '}', '$');

            return whileNot
                + MaybeMany(
                    Any(
                        NamedGroup("text", Snippets.CSharpEscapedTextLiteral()),
                        NamedGroup("text", Snippets.CSharpVerbatimTextLiteral()),
                        NamedGroup("char", Snippets.CSharpCharacterLiteral()),
                        NamedGroup("comment", Snippets.CSharpMultilineComment())
                    )
                    + whileNot
                );
        }
    }
}