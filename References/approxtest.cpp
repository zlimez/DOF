#include <cmath>
#include <vector>
#include <iostream>

#define PI 3.14159265359
#define HPI 1.57079632679

using namespace std;

float measureExec(float (*func)(float), float x)
{
    clock_t start, end;
    start = clock();
    float res = func(x);
    end = clock();
    double time_taken = double(end - start) / double(CLOCKS_PER_SEC);
    cout << "Time taken by function: " << time_taken << " seconds" << endl;
    return res;
}

void batchMeasureExec(float (*func)(float), vector<float> inputs)
{
    clock_t start, end;
    start = clock();
    for (int i = 0; i < inputs.size(); i++)
    {
        func(inputs[i]);
    }
    end = clock();
    double time_taken = double(end - start) / double(CLOCKS_PER_SEC);
    cout << "Time taken by function: " << time_taken << " seconds" << endl;
}

void batchMeasureExec(float (*func)(float, float), vector<pair<float, float>> inputs)
{
    clock_t start, end;
    start = clock();
    for (int i = 0; i < inputs.size(); i++)
    {
        func(inputs[i].first, inputs[i].second);
    }
    end = clock();
    double time_taken = double(end - start) / double(CLOCKS_PER_SEC);
    cout << "Time taken by function: " << time_taken << " seconds" << endl;
}

float fSin(float x)
{
    int mult = 1;
    if (x > HPI)
    {
        if (x <= PI)
        {
            x = PI - x;
        }
        else if (x <= PI + HPI)
        {
            mult = -1;
            x -= PI;
        }
        else
        {
            x = 2 * PI - x;
            mult = -1;
        }
    }
    float x2 = x * x;
    return mult * (x - (x2 * x * 0.16666667) + (x2 * x2 * x * 0.00833333));
}

float fAtan2(float y, float x) {
    float qdrPfix = 0, absx = abs(x), absy = abs(y);
    int sign = 1;
    float ratio = absy > absx ? absx / absy : absy / absx;
    // cout << " ratio = " << ratio << " x = " << x << " y = " << y << endl;
    if (x >= 0 && y >= 0 && y > x) {
        qdrPfix = HPI;
        sign = -1;
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
    const float a = 0.33333333, b = 0.2, c = 0.14285714;

    float r2 = ratio * ratio, r3 = r2 * ratio, r5 = r2 * r3, r7 = r2 * r5;
    return qdrPfix + sign * (ratio - a * r3 + b * r2 * r3 - c * r7);
}

float fCos(float x)
{
    return fSin(x + HPI);
}

void timeMeasureAtan2() {
    vector<pair<float, float>> inputs;
    for (int i = 0; i < 720; i++)
    {
        float angle = i * PI / 180 / 2;
        inputs.push_back({sin(angle), cos(angle)});
    }
    batchMeasureExec(atan2, inputs);
    batchMeasureExec(fAtan2, inputs);
}

void accMeasureAtan2() {
    for (int i = 0; i < 360; i += 15)
    {
        float angle = i * PI / 180;
        printf("lib atan2 = %f", angle);
        printf(" fatan2 = %f\n", fAtan2(sin(angle), cos(angle)));
    }
}

void timeMeasureSinCos() {
    vector<float> inputs;
    for (int i = 0; i < 1080; i++)
    {
        inputs.push_back(i * PI / 180 / 3);
    }
    batchMeasureExec(sin, inputs);
    batchMeasureExec(cos, inputs);
    batchMeasureExec(fSin, inputs);
    batchMeasureExec(fCos, inputs);
}

void accMeasureSinCos() {
    vector<float> inputs = {0, 30, 45, 60, 90, 120, 135, 150, 180, 210, 225, 240, 270, 300, 315, 330, 360};

    for (int i = 0; i < inputs.size(); i++)
    {
        float x = inputs[i] * PI / 180;

        float sinVal = fSin(x), cosVal = fCos(x);
        float lsin = sin(x), lcos = cos(x);
        printf("sin(%f) = %f, cos(%f) = %f\n", x, sinVal, x, cosVal);
        printf("sin(%f) = %f, cos(%f) = %f\n", x, sin(x), x, cos(x));
    }
}

int main()
{
    // timeMeasureAtan2();
    timeMeasureSinCos();
    // accMeasureAtan2();
    // accMeasureSinCos();
}
