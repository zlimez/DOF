#define PI 3.14159265359
#define HPI 1.57079632679

// Fast approx of sin with Taylor expansion
float fSin(float x)
{
    int mult = 1;

    if (x > HPI) {
        if (x <= PI) {
            x = PI - x;
        } else if (x <= PI + HPI) {
            mult = -1;
            x -= PI;
        } else {
            x = 2 * PI - x;
            mult = -1;
        }
    }

    float x2 = x * x; float x3 = x2 * x;
    return mult * (x - x3 * 0.16666667) + (x2 * x3 * 0.00833333));
}

float fCos(float x)
{
    return fSin(x + HPI);
}

float2 fSinCos(float x)
{
    return float2(fSin(x), fCos(x));
}

float fAtan2(float y, float x) {
    float qdrPfix = 0;
    int sign = 1, absx = abs(x), absy = abs(y);
    float ratio = absy > absx ? absx / absy : absy / absx;
    if (x >= 0 && y >= 0 && y > x) {
        qdrPfix = HPI;
        sign = -1;
        ratio = x / y;
    } else if (x < 0 && y >= 0) {
        if (y > -x) {
            qdrPfix = HPI;
        } else {
            qdrPfix = PI;
            sign = -1;
        }
    } else if (x < 0 && y < 0) {
        if (-y > -x) {
            qdrPfix = PI + HPI;
            ratio = x / y;
            sign = -1;
        } else {
            qdrPfix = PI;
        }
    } else if (x >= 0 && y < 0) {
        if (-y > x) {
            qdrPfix = PI + HPI;
        } else {
            qdrPfix = 2 * PI;
            sign = -1;
        }
    }
    // Coefficients for the polynomial approximation
    const float a = 0.33333333, b = 0.2;

    float r2 = ratio * ratio; float r3 = r2 * ratio;
    return qdrPfix + sign * (x - a * r3 + b * r2 * r3);
}