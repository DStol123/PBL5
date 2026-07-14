import matplotlib.pyplot as plt
import pandas as pd
import numpy as np
from pathlib import Path
import re

# ============================================================
# Configuration
# ============================================================

TRIAL_LOG = "newTrialLogs.csv"

# Independent variables
BUBBLE_COL = "Bubbles"
AH_COL = "AH"
LI_COL = "LI"

# Timing variables
TIME_BOTTOM = "Time to Bottom"
TIME_RETURN = "Time to Return"

# Time series files
TIME_SERIES_PATTERN = "*TimeSeries.csv"

# Ignore tiny movements (meters)
DEPTH_THRESHOLD = 0.05


# ============================================================
# Load Trial Log
# ============================================================

def load_trial_log():

    df = pd.read_csv(TRIAL_LOG)

    # Remove whitespace around column names
    df.columns = df.columns.str.strip()

    if TIME_BOTTOM in df.columns:
        df[TIME_BOTTOM] = df[TIME_BOTTOM].replace(9999, np.nan)

    # Treat failed returns as missing
    if TIME_RETURN in df.columns:
        df[TIME_RETURN] = df[TIME_RETURN].replace(9999, np.nan)

    # Total time
    df["Total Time"] = df[TIME_BOTTOM] + df[TIME_RETURN]

    return df


# ============================================================
# Automatically discover sickness variables
# ============================================================

def discover_before_after_pairs(df):

    pairs = {}

    for col in df.columns:

        # Example:
        # Headache Before
        # Headache After

        if col.endswith(" Before"):

            base = col[:-7]
            after = base + " After"

            if after in df.columns:
                pairs[base] = (col, after)

        # Example:
        # NauseaBefore
        # NauseaAfter

        elif col.endswith("Before"):

            base = col[:-6]
            after = base + "After"

            if after in df.columns:
                pairs[base] = (col, after)

    return pairs


# ============================================================
# Compute questionnaire changes
# ============================================================

def compute_change_scores(df, before_after_pairs):

    for variable, (before, after) in before_after_pairs.items():

        df[variable + " Change"] = df[after] - df[before]


# ============================================================
# Detect NASA columns
# ============================================================

def discover_nasa_columns(df):

    return [
        c for c in df.columns
        if c.startswith("NASA ")
    ]


# ============================================================
# Find all TimeSeries files
# ============================================================

def get_time_series_files():

    files = sorted(Path(".").glob(TIME_SERIES_PATTERN))

    return files


# ============================================================
# Match a TimeSeries file to its row in trial log
# ============================================================

def find_trial_row(df, filename):

    """
    Example:
        1aTimeSeries.csv

    participant = 1
    trial = a
    """

    match = re.match(r"(\d+)([a-z])TimeSeries\.csv", filename)

    if match is None:
        return None

    participant = int(match.group(1))
    trial_letter = match.group(2)

    # a=1, b=2, c=3...
    trial_number = ord(trial_letter) - ord("a") + 1

    participant_rows = df[df["Participant"] == participant]

    if len(participant_rows) < trial_number:
        return None

    return participant_rows.index[trial_number - 1]


# ============================================================
# Navigation metrics
# ============================================================

def compute_navigation_metrics(timeseries_df, bottom_time):

    time = timeseries_df["Seconds"].to_numpy()
    depth = timeseries_df["Depth"].to_numpy()

    correct_time = 0.0
    stationary_time = 0.0
    vertical_distance = 0.0

    direction_reversals = 0
    previous_direction = 0

    weighted_speed = []

    total_time = time[-1] - time[0]

    for i in range(len(time) - 1):

        dt = time[i + 1] - time[i]

        if dt <= 0:
            continue

        delta_depth = depth[i + 1] - depth[i]

        vertical_distance += abs(delta_depth)

        # Ignore tiny movement
        if abs(delta_depth) < DEPTH_THRESHOLD:

            stationary_time += dt
            continue

        # Current direction
        direction = np.sign(delta_depth)

        if previous_direction != 0 and direction != previous_direction:
            direction_reversals += 1

        previous_direction = direction

        speed = abs(delta_depth) / dt
        weighted_speed.append(speed)

        # -----------------------------
        # Going DOWN
        # -----------------------------
        if np.isnan(bottom_time) or time[i] < bottom_time:

            if delta_depth > DEPTH_THRESHOLD:
                correct_time += dt

        # -----------------------------
        # Going UP
        # -----------------------------
        else:

            if delta_depth < -DEPTH_THRESHOLD:
                correct_time += dt

    proportion_correct = np.nan

    if total_time > 0:
        proportion_correct = min(correct_time / (total_time - stationary_time), 1)

    metrics = {
        "Correct Direction %": proportion_correct * 100,
        "Vertical Distance": vertical_distance,
        "Average Vertical Speed":
            np.mean(weighted_speed) if len(weighted_speed) else np.nan,
        "Direction Reversals": direction_reversals,
        "Stationary Time": stationary_time
    }

    return metrics


# ============================================================
# Compute metrics for every trial
# ============================================================

def compute_all_navigation_metrics(df):

    metric_names = [
        "Correct Direction %",
        "Vertical Distance",
        "Average Vertical Speed",
        "Direction Reversals",
        "Stationary Time"
    ]

    for metric in metric_names:
        df[metric] = np.nan

    for file in get_time_series_files():

        row = find_trial_row(df, file.name)

        if row is None:
            print(f"Couldn't match {file.name}")
            continue

        ts = pd.read_csv(file)

        ts.columns = ts.columns.str.strip()

        metrics = compute_navigation_metrics(
            ts,
            df.loc[row, TIME_BOTTOM]
        )

        for key, value in metrics.items():
            df.loc[row, key] = value

    return df

# ============================================================
# Experimental Conditions
# ============================================================

CONDITIONS = [
    ("Control", False, False),
    ("AH", True, False),
    ("LI", False, True)
]

BUBBLE_ROWS = [
    ("No Bubbles", False),
    ("Bubbles", True)
]


# ============================================================
# Compute Mean +- SD Table
# ============================================================

def build_condition_table(df, variable):

    table = []

    for row_name, bubbles in BUBBLE_ROWS:

        row = []

        for column_name, ah, li in CONDITIONS:

            subset = df[
                (df[BUBBLE_COL] == bubbles) &
                (df[AH_COL] == ah) &
                (df[LI_COL] == li)
            ][variable].dropna()

            if len(subset) == 0:
                row.append("-")
                continue

            mean = subset.mean()
            std = subset.std(ddof=1)
            n = len(subset)

            row.append(f"mean {mean:.2f}\nstd {std:.2f} (n={n})")

        table.append(row)

    return table


# ============================================================
# Save One Table
# ============================================================

def save_table(table, title, filename):

    fig, ax = plt.subplots(figsize=(8, 3.5))
    ax.axis("off")

    mpl_table = ax.table(
        cellText=table,
        rowLabels=[r[0] for r in BUBBLE_ROWS],
        colLabels=[c[0] for c in CONDITIONS],
        cellLoc="center",
        loc="center"
    )

    mpl_table.auto_set_font_size(False)
    mpl_table.set_fontsize(11)
    mpl_table.scale(1.35, 2.4)

    # Header formatting
    for (row, col), cell in mpl_table.get_celld().items():

        if row == 0:
            cell.set_text_props(weight="bold")

        if col == -1:
            cell.set_text_props(weight="bold")

    plt.title(title, fontsize=14, pad=15)

    plt.savefig(
        filename,
        dpi=300,
        bbox_inches="tight"
    )

    plt.close()


# ============================================================
# Convenience Function
# ============================================================

def create_condition_table(df, variable, output_folder="Tables"):

    Path(output_folder).mkdir(exist_ok=True)

    table = build_condition_table(df, variable)

    filename = (
        Path(output_folder)
        / f"{variable.replace('/', '_').replace(' ', '_')}.png"
    )

    save_table(
        table,
        variable,
        filename
    )


# ============================================================
# Automatically Create Tables
# ============================================================

def create_all_tables(
        df,
        sickness_pairs,
        nasa_columns):

    variables = [
        TIME_BOTTOM,
        TIME_RETURN,
        "Total Time"
    ]

    # Add sickness changes
    for variable in sickness_pairs.keys():
        variables.append(variable + " Change")

    # Add NASA scores
    variables.extend(nasa_columns)

    # Add navigation metrics
    navigation_metrics = [
        "Correct Direction %",
        "Vertical Distance",
        "Average Vertical Speed",
        "Direction Reversals",
        "Stationary Time"
    ]

    for metric in navigation_metrics:
        if metric in df.columns:
            variables.append(metric)

    # Generate every table
    for variable in variables:

        if variable not in df.columns:
            print(f"Skipping '{variable}' (not found)")
            continue

        print(f"Creating table for {variable}")

        create_condition_table(df, variable)

# ============================================================
# Main
# ============================================================

def main():

    print("Loading trial log...")
    df = load_trial_log()

    print(f"Loaded {len(df)} trials.")

    # --------------------------------------------------------
    # Discover variables
    # --------------------------------------------------------

    print("Discovering sickness variables...")

    sickness_pairs = discover_before_after_pairs(df)

    if len(sickness_pairs) == 0:
        print("No before/after sickness variables found.")
    else:
        print("Found sickness variables:")
        for variable in sickness_pairs.keys():
            print(f"  {variable}")

    print()

    nasa_columns = discover_nasa_columns(df)

    if len(nasa_columns) == 0:
        print("No NASA variables found.")
    else:
        print("Found NASA variables:")
        for variable in nasa_columns:
            print(f"  {variable}")

    print()

    # --------------------------------------------------------
    # Compute derived variables
    # --------------------------------------------------------

    print("Computing change scores...")
    compute_change_scores(df, sickness_pairs)

    # --------------------------------------------------------
    # Navigation metrics
    # --------------------------------------------------------

    print("Analyzing TimeSeries files...")
    df = compute_all_navigation_metrics(df)

    print()

    # --------------------------------------------------------
    # Save processed data
    # --------------------------------------------------------

    output_csv = "ProcessedTrialLogs.csv"

    df.to_csv(output_csv, index=False)

    print(f"Saved processed data to {output_csv}")

    # --------------------------------------------------------
    # Generate summary tables
    # --------------------------------------------------------

    print("Generating summary tables...")

    create_all_tables(
        df,
        sickness_pairs,
        nasa_columns
    )

    print()

    print("Finished!")
    print("Tables saved to ./Tables/")


# ============================================================
# Entry Point
# ============================================================

if __name__ == "__main__":
    main()