' Xoshiro256StarStarRandom.vb
' VB.NET / .NET Framework 4.7 compatible implementation of xoshiro256** 1.0.
' Based on the public-domain reference implementation by David Blackman and
' Sebastiano Vigna: https://prng.di.unimi.it/xoshiro256starstar.c
'
' This is a fast, deterministic PRNG for simulation / Monte Carlo work.
' It is not cryptographically secure. Use System.Security.Cryptography for
' secrets, tokens, keys, passwords, or adversarial settings.
'
' The generator uses modulo-2^64 arithmetic. The small wrapping helpers below
' make that behavior explicit, so the class works even when VB integer overflow
' checks are enabled.

Option Strict On
Option Explicit On

Imports System
Imports System.Security.Cryptography

Public NotInheritable Class Xoshiro256StarStarRandom
    Private _s0 As ULong
    Private _s1 As ULong
    Private _s2 As ULong
    Private _s3 As ULong

    Private Const DoubleUnit As Double = 1.0R / 9007199254740992.0R ' 2^-53

    Private Shared ReadOnly JumpConstants As ULong() = {
        &H180EC6D33CFD0ABAUL,
        &HD5A61266F0C9392CUL,
        &HA9582618E03FC9AAUL,
        &H39ABDC4529B1661CUL
    }

    Private Shared ReadOnly LongJumpConstants As ULong() = {
        &H76E15D3EFEFDCBBFUL,
        &HC5004E441C522FB3UL,
        &H77710069854EE241UL,
        &H39109BB02ACBE635UL
    }

    Public Sub New()
        Dim seedBytes(31) As Byte

        Using rng As RandomNumberGenerator = RandomNumberGenerator.Create()
            rng.GetBytes(seedBytes)
        End Using

        _s0 = ReadUInt64LittleEndian(seedBytes, 0)
        _s1 = ReadUInt64LittleEndian(seedBytes, 8)
        _s2 = ReadUInt64LittleEndian(seedBytes, 16)
        _s3 = ReadUInt64LittleEndian(seedBytes, 24)

        If IsAllZero() Then
            _s0 = 1UL
        End If
    End Sub

    Public Sub New(seed As ULong)
        Dim sm As New SplitMix64(seed)

        _s0 = sm.NextUInt64()
        _s1 = sm.NextUInt64()
        _s2 = sm.NextUInt64()
        _s3 = sm.NextUInt64()

        If IsAllZero() Then
            _s0 = 1UL
        End If
    End Sub

    Public Sub New(s0 As ULong, s1 As ULong, s2 As ULong, s3 As ULong)
        If (s0 Or s1 Or s2 Or s3) = 0UL Then
            Throw New ArgumentException("The xoshiro256** state cannot be all zero.")
        End If

        _s0 = s0
        _s1 = s1
        _s2 = s2
        _s3 = s3
    End Sub

    Public Function Clone() As Xoshiro256StarStarRandom
        Return New Xoshiro256StarStarRandom(_s0, _s1, _s2, _s3)
    End Function

    Public Function State() As ULong()
        Return New ULong() {_s0, _s1, _s2, _s3}
    End Function

    Public Function NextUInt64() As ULong
        Dim result As ULong = Mul9(RotL(Mul5(_s1), 7))
        Dim t As ULong = _s1 << 17

        _s2 = _s2 Xor _s0
        _s3 = _s3 Xor _s1
        _s1 = _s1 Xor _s2
        _s0 = _s0 Xor _s3

        _s2 = _s2 Xor t
        _s3 = RotL(_s3, 45)

        Return result
    End Function

    Public Function NextUInt32() As UInteger
        Return CUInt(NextUInt64() >> 32)
    End Function

    ' Returns a uniformly distributed IEEE-754 Double in [0, 1).
    Public Function NextDouble() As Double
        Return CDbl(NextUInt64() >> 11) * DoubleUnit
    End Function

    Public Sub NextBytes(buffer As Byte())
        If buffer Is Nothing Then
            Throw New ArgumentNullException("buffer")
        End If

        Dim index As Integer = 0

        While index < buffer.Length
            Dim value As ULong = NextUInt64()
            Dim count As Integer = Math.Min(8, buffer.Length - index)

            For i As Integer = 0 To count - 1
                buffer(index + i) = CByte(value And &HFFUL)
                value >>= 8
            Next

            index += count
        End While
    End Sub

    ' Equivalent to 2^128 calls to NextUInt64(); useful for non-overlapping
    ' streams in parallel computations.
    Public Sub Jump()
        ApplyJump(JumpConstants)
    End Sub

    ' Equivalent to 2^192 calls to NextUInt64(); useful for distributed jobs.
    Public Sub LongJump()
        ApplyJump(LongJumpConstants)
    End Sub

    Private Sub ApplyJump(jumpTable As ULong())
        Dim s0 As ULong = 0UL
        Dim s1 As ULong = 0UL
        Dim s2 As ULong = 0UL
        Dim s3 As ULong = 0UL

        For Each jumpValue As ULong In jumpTable
            For bit As Integer = 0 To 63
                If (jumpValue And (1UL << bit)) <> 0UL Then
                    s0 = s0 Xor _s0
                    s1 = s1 Xor _s1
                    s2 = s2 Xor _s2
                    s3 = s3 Xor _s3
                End If

                NextUInt64()
            Next
        Next

        _s0 = s0
        _s1 = s1
        _s2 = s2
        _s3 = s3
    End Sub

    Private Function IsAllZero() As Boolean
        Return (_s0 Or _s1 Or _s2 Or _s3) = 0UL
    End Function

    Private Shared Function RotL(x As ULong, k As Integer) As ULong
        Return (x << k) Or (x >> (64 - k))
    End Function

    Private Shared Function Add64(a As ULong, b As ULong) As ULong
        Const Mask32 As ULong = &HFFFFFFFFUL

        Dim low As ULong = (a And Mask32) + (b And Mask32)
        Dim high As ULong = (a >> 32) + (b >> 32) + (low >> 32)

        Return ((high And Mask32) << 32) Or (low And Mask32)
    End Function

    Private Shared Function Mul5(x As ULong) As ULong
        Return Add64(x << 2, x)
    End Function

    Private Shared Function Mul9(x As ULong) As ULong
        Return Add64(x << 3, x)
    End Function

    Private Shared Function Mul64(a As ULong, b As ULong) As ULong
        Const Mask32 As ULong = &HFFFFFFFFUL

        Dim a0 As ULong = a And Mask32
        Dim a1 As ULong = a >> 32
        Dim b0 As ULong = b And Mask32
        Dim b1 As ULong = b >> 32

        Dim lowProduct As ULong = a0 * b0
        Dim crossLow As ULong = ((a0 * b1) And Mask32) + ((a1 * b0) And Mask32)

        Return Add64(lowProduct, (crossLow And Mask32) << 32)
    End Function

    Private Shared Function ReadUInt64LittleEndian(bytes As Byte(), offset As Integer) As ULong
        Return CULng(bytes(offset)) Or
               (CULng(bytes(offset + 1)) << 8) Or
               (CULng(bytes(offset + 2)) << 16) Or
               (CULng(bytes(offset + 3)) << 24) Or
               (CULng(bytes(offset + 4)) << 32) Or
               (CULng(bytes(offset + 5)) << 40) Or
               (CULng(bytes(offset + 6)) << 48) Or
               (CULng(bytes(offset + 7)) << 56)
    End Function

    Private Structure SplitMix64
        Private _state As ULong

        Private Const Gamma As ULong = &H9E3779B97F4A7C15UL
        Private Const Multiplier1 As ULong = &HBF58476D1CE4E5B9UL
        Private Const Multiplier2 As ULong = &H94D049BB133111EBUL

        Public Sub New(seed As ULong)
            _state = seed
        End Sub

        Public Function NextUInt64() As ULong
            _state = Add64(_state, Gamma)

            Dim z As ULong = _state
            z = Mul64(z Xor (z >> 30), Multiplier1)
            z = Mul64(z Xor (z >> 27), Multiplier2)
            Return z Xor (z >> 31)
        End Function
    End Structure
End Class
