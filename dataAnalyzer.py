import pandas as pd
from scipy.stats import ttest_ind

# ============================================================
# Configuration
# ============================================================

trialLogs = "newTrialLogs.csv"

# ============================================================
# Load Data
# ============================================================

def load_data(csv_file):
    df = pd.read_csv(csv_file)

    bool_columns = [
        "Bubbles",
        "AH",
        "LI",
        "Glasses/Contacts"
    ]

    for col in bool_columns:
        if col in df.columns:
            df[col] = (
                df[col]
                .astype(str)
                .str.upper()
                .map({"TRUE": True, "FALSE": False})
            )

    return df


# ============================================================
# Create Experimental Condition
# ============================================================

def add_condition_column(df):

    def determine(row):

        if row["AH"]:
            condition = "AH"

        elif row["LI"]:
            condition = "Lights"

        else:
            condition = "Control"

        if row["Bubbles"]:
            condition += " + Bubbles"

        return condition

    df["Condition"] = df.apply(determine, axis=1)

    return df


# ============================================================
# Calculate Symptom Changes
# ============================================================

def calculate_changes(df):

    df["Headache Change"] = (
        df["Headache After"] - df["Headache Before"]
    )

    df["Nausea Change"] = (
        df["NauseaAfter"] - df["Nausea Before"]
    )

    df["Dizziness Change"] = (
        df["DizzinessAfter"] - df["DizzinessBefore"]
    )

    return df


# ============================================================
# Overall Statistics
# ============================================================

def overall_statistics(df):

    print("=" * 80)
    print("OVERALL DATASET SUMMARY")
    print("=" * 80)

    print(f"Rows: {len(df)}")
    print(f"Participants: {df['Participant'].nunique()}")

    print("\nSessions per participant")
    print(df["Participant"].value_counts().sort_index())

    print("\nNumeric Statistics")
    print("-" * 80)

    numeric = df.select_dtypes(include="number")
    print(numeric.describe().T)

    print("\nBoolean Counts")
    print("-" * 80)

    bool_cols = df.select_dtypes(include="bool").columns

    for col in bool_cols:
        print(f"\n{col}")
        print(df[col].value_counts())

    print("\nCategorical Counts")
    print("-" * 80)

    for col in ["Gender", "Condition"]:
        if col in df.columns:
            print(f"\n{col}")
            print(df[col].value_counts())


# ============================================================
# Statistics by Experimental Condition
# ============================================================

def condition_statistics(df):

    dependent = [
        "Time to Bottom",
        "Time to Return",
        "Time to Become Nauseous",
        "Headache Before",
        "Headache After",
        "Headache Change",
        "Nausea Before",
        "NauseaAfter",
        "Nausea Change",
        "DizzinessBefore",
        "DizzinessAfter",
        "Dizziness Change"
    ]

    print("\n")
    print("=" * 80)
    print("STATISTICS BY CONDITION")
    print("=" * 80)

    print(
        df.groupby("Condition")[dependent]
          .agg(["mean", "std", "median", "min", "max", "count"])
    )


# ============================================================
# Participant Statistics
# ============================================================

def participant_statistics(df, participant_id):

    participant = df[df["Participant"] == participant_id]

    if participant.empty:
        print(f"No data for participant {participant_id}")
        return

    print("\n")
    print("=" * 80)
    print(f"PARTICIPANT {participant_id}")
    print("=" * 80)

    print(f"Sessions: {len(participant)}")

    print("\nConditions")
    print(participant["Condition"].value_counts())

    print("\nDates")
    print(participant["Date"].tolist())

    print("\nNumeric Summary")
    print("-" * 80)

    numeric = participant.select_dtypes(include="number")
    print(numeric.describe().T)

    print("\nBoolean Variables")
    print("-" * 80)

    bool_cols = participant.select_dtypes(include="bool").columns

    for col in bool_cols:
        print(f"\n{col}")
        print(participant[col].value_counts())

    print("\nSymptom Changes")
    print("-" * 80)

    comparisons = [
        ("Headache Before", "Headache After"),
        ("Nausea Before", "NauseaAfter"),
        ("DizzinessBefore", "DizzinessAfter")
    ]

    for before, after in comparisons:

        print(f"\n{before} -> {after}")

        print(f"Mean Before : {participant[before].mean():.2f}")
        print(f"Mean After  : {participant[after].mean():.2f}")
        print(
            f"Mean Change : {(participant[after]-participant[before]).mean():.2f}"
        )


# ============================================================
# Participant Comparison
# ============================================================

def participant_summary(df):

    print("\n")
    print("=" * 80)
    print("PARTICIPANT AVERAGES")
    print("=" * 80)

    summary = (
        df.groupby("Participant")[[
            "Time to Bottom",
            "Time to Return",
            "Headache Change",
            "Nausea Change",
            "Dizziness Change"
        ]]
        .mean()
    )

    print(summary)


# ============================================================
# Correlation Matrix
# ============================================================

def correlation_matrix(df):

    print("\n")
    print("=" * 80)
    print("CORRELATION MATRIX")
    print("=" * 80)

    numeric = df.select_dtypes(include="number")

    print(
        numeric.corr(numeric_only=True).round(2)
    )


# ============================================================
# Statistical Tests
# ============================================================

def statistical_tests(df):

    print("\n")
    print("=" * 80)
    print("INFERENTIAL STATISTICS")
    print("=" * 80)

    dependent = [
        "Time to Bottom",
        "Time to Return",
        "Headache Change",
        "Nausea Change",
        "Dizziness Change"
    ]

    # AH vs Lights
    print("\nAH vs Lights")

    ah = df[df["AH"]]
    lights = df[df["LI"]]

    for variable in dependent:

        if len(ah) > 1 and len(lights) > 1:

            stat, p = ttest_ind(
                ah[variable],
                lights[variable],
                equal_var=False,
                nan_policy="omit"
            )

            print(f"{variable:25s} p = {p:.4f}")

    # Bubble vs No Bubble
    print("\nBubbles vs No Bubbles")

    bubbles = df[df["Bubbles"]]
    no_bubbles = df[~df["Bubbles"]]

    for variable in dependent:

        if len(bubbles) > 1 and len(no_bubbles) > 1:

            stat, p = ttest_ind(
                bubbles[variable],
                no_bubbles[variable],
                equal_var=False,
                nan_policy="omit"
            )

            print(f"{variable:25s} p = {p:.4f}")


# ============================================================
# Main
# ============================================================

if __name__ == "__main__":

    df = load_data(trialLogs)

    df = add_condition_column(df)

    df = calculate_changes(df)

    overall_statistics(df)

    condition_statistics(df)

    participant_summary(df)

    correlation_matrix(df)

    statistical_tests(df)

    # Change this participant ID as required
    participant_statistics(df, participant_id=1)