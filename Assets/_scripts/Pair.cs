using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pair<T> {
    public T left;
    public T right;

    public Pair (T i, T i2) {
        left = i;
        right = i2;
    }

    public override string ToString () {
        return "(" + left + ", " + right + ")";
    }

    public bool Contains (T i) {
        return left.Equals (i) || right.Equals (i);
    }

    public bool SameAs (Pair<T> p) {
        if (p == null) {
            return false;
        }
        return (p.left.Equals (left) && p.right.Equals (right)) || (p.left.Equals (right) && p.right.Equals (left));
    }

    public Pair<T> Reversed () {
        return new Pair<T> (this.right, this.left);
    }

    public void Reverse () {
        var temp = this.left;
        this.left = right;
        right = temp;
    }

    public static T GetSame (Pair<T> p1, Pair<T> p2) {
        if (p1.left.Equals (p1.left)) {
            return p1.left;
        } else if (p1.right.Equals (p2.right)) {
            return p1.right;
        } else if (p1.left.Equals (p2.right)) {
            return p1.left;
        } else if (p1.right.Equals (p2.left)) {
            return p1.right;
        }
        return default (T);
    }

    public bool Equals (Pair<T> other) {
        return other.left.Equals (left) && other.right.Equals (right);
    }

    public override bool Equals (object obj) {
        return Equals (obj as Pair<T>);
    }

    public override int GetHashCode () {
        var hashCode = -124503083;
        hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode (left);
        hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode (right);
        return hashCode;
    }
}